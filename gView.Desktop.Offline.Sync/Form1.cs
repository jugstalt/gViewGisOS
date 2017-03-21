using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using gView.Framework.system;
using gView.Framework.Data;
using gView.Framework.Offline;
using System.Threading;
using gView.Framework.system.UI;

namespace gView.Desktop.Offline.Sync
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region EventHandler
        private void Form1_Load(object sender, EventArgs e)
        {
#if(!DEBUG)
            SplashScreen splash = new SplashScreen("Sync 2.0", false, SystemVariables.gViewVersion);
            Thread thread = new Thread(new ParameterizedThreadStart(this.SplashScreen));

            thread.Start(splash);

            Thread sleeper = new Thread(new ParameterizedThreadStart(this.Sleeper));
            sleeper.Start(splash);
#endif
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(SystemVariables.ApplicationDirectory + @"\sync.xml");

                foreach (XmlNode node in doc.SelectNodes("sync/datasets/dataset[@name]"))
                {
                    if (node.Attributes["pluginguid"] == null ||
                        node.Attributes["connectionstring"] == null) continue;

                    cmbDatasets.Items.Add(new DatasetItem(
                        node.Attributes["name"].Value,
                        new Guid(node.Attributes["pluginguid"].Value),
                        node.Attributes["connectionstring"].Value));
                }
                if (cmbDatasets.Items.Count > 0)
                    cmbDatasets.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
                this.Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = (_thread != null);
        }
        
        private void cmbDatasets_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstFeatureclasses.Items.Clear();
            if (cmbDatasets.SelectedIndex == -1) return;

            IFeatureDataset dataset = ((DatasetItem)cmbDatasets.SelectedItem).Dataset;
            if (dataset == null || !(dataset.Database is IFeatureDatabaseReplication)) return;
            IFeatureDatabaseReplication db = (IFeatureDatabaseReplication)dataset.Database;

            foreach (IDatasetElement element in dataset.Elements)
            {
                if (element == null || !(element.Class is IFeatureClass)) continue;
                IFeatureClass fc = (IFeatureClass)element.Class;

                //List<Guid> SessionGuids = Replication.FeatureClassSessions(fc);
                //if (SessionGuids == null) continue;

                int generation = Replication.FeatureClassGeneration(fc);
                if (generation < 1) continue;

                lstFeatureclasses.Items.Add(new FeatureClassItem(fc));
            }
        }

        private void lstFeatureclasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFeatureclasses.SelectedItems.Count == 0) return;

            FeatureClassItem item = (FeatureClassItem)lstFeatureclasses.SelectedItems[0];
            txtMessage.Text = item.MessageString;
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            if (_thread != null)
            {
                MessageBox.Show("Thread is running!!");
                return;
            }
            _thread = new Thread(new ThreadStart(CheckinProcess));

            _fcs = new List<IFeatureClass>();
            foreach (FeatureClassItem fcItem in lstFeatureclasses.Items)
            {
                fcItem.ImageIndex = 0;
                fcItem.MessageString = String.Empty;
                if (fcItem.Checked == false || fcItem.FeatureClass == null) continue;
                _fcs.Add(fcItem.FeatureClass);
            }

            btnSync.Enabled = btnCancel.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            _thread.Start();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_thread == null)
                this.Close();
        }
        #endregion

        #region Worker Thread
        List<IFeatureClass> _fcs = null;
        IFeatureClass _processedFc = null;
        Thread _thread = null;
        int _inserted = 0, _updated = 0, _deleted = 0, _conflicts = 0;
        StringBuilder _resultString = new StringBuilder();
        private void CheckinProcess()
        {
            try
            {
                if (_fcs == null || _fcs.Count == 0) return;

                string errMsg = String.Empty;
                foreach (IFeatureClass fc in _fcs)
                {
                    _resultString = new StringBuilder();
                    _inserted = 0; _updated = 0; _deleted = 0; _conflicts = 0;
                    _processedFc = fc;
                    SetListItemImage(fc, 1);

                    IFeatureClass parentFc = Replication.FeatureClassParentFc(fc, out errMsg);
                    if (parentFc == null)
                    {
                        SetItemText(fc, errMsg, true, true);
                        SetListItemImage(fc, 3);
                        continue;
                    }

                    Replication repl = new Replication();
                    repl.CheckIn_FeatureInserted += new Replication.CheckIn_FeatureInsertedEventHandler(repl_CheckIn_FeatureInserted);
                    repl.CheckIn_FeatureUpdated += new Replication.CheckIn_FeatureUpdatedEventHandler(repl_CheckIn_FeatureUpdated);
                    repl.CheckIn_FeatureDeleted += new Replication.CheckIn_FeatureDeletedEventHandler(repl_CheckIn_FeatureDeleted);
                    repl.CheckIn_ConflictDetected += new Replication.CheckIn_ConflictDetectedEventHandler(repl_CheckIn_ConflictDetected);
                    repl.CheckIn_BeginPost += new Replication.CheckIn_BeginPostEventHandler(repl_CheckIn_BeginPost);
                    repl.CheckIn_Message += new Replication.CheckIn_MessageEventHandler(repl_CheckIn_Message);
                    SetItemText(fc, "", true, true);
                    SetItemText(fc, "Checkin:", true, true);
                    Replication.ProcessType type = Replication.ProcessType.Reconcile;
                    if (!repl.Process(parentFc, fc, type, out errMsg))
                    {
                        SetItemText(fc, errMsg, true, true);
                        SetListItemImage(fc, 3);
                    }
                    else
                    {
                        _resultString.Append("Reconcile:\t" + _inserted + "\t" + _updated + "\t" + _deleted + "\t" + _conflicts);
                        SetItemText(fc, _resultString.ToString(), false, false);
                        SetListItemImage(fc, 2);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                ThreadFinished();
                _thread = null;
            }
        }

        void repl_CheckIn_Message(Replication sender, string message)
        {
            SetItemText(_processedFc, message, true, true);
        }

        void repl_CheckIn_FeatureInserted(Replication sender, int count_inserted)
        {
            _inserted++;
            SetItemText(_processedFc, "..i", true, false);
        }

        void repl_CheckIn_FeatureUpdated(Replication sender, int count_updated)
        {
            _updated++;
            SetItemText(_processedFc, "..u", true, false);
        }

        void repl_CheckIn_FeatureDeleted(Replication sender, int count_deleted)
        {
            _deleted++;
            SetItemText(_processedFc, "..d", true, false);
        }

        void repl_CheckIn_ConflictDetected(Replication sender, int count_confilicts)
        {
            _conflicts++;
            SetItemText(_processedFc, "..c", true, false);
        }

        void repl_CheckIn_BeginPost(Replication sender)
        {
            _resultString.Append("\t\tINSERT\tUPDATE\tDELETE\tCONFLICTS\r\n");
            _resultString.Append("-------------------------------------------------------------\r\n");
            _resultString.Append("Checkin  :\t" + _inserted + "\t" + _updated + "\t" + _deleted + "\t" + _conflicts+"\r\n");

            _inserted = 0; _updated = 0; _deleted = 0; _conflicts = 0;
            SetItemText(_processedFc, "", true, true);
            SetItemText(_processedFc, "Reconcile:", true, true);
        }

        #endregion

        #region GUI
        private delegate void SetListItemImage_Callback(IFeatureClass fc, int index);
        private void SetListItemImage(IFeatureClass fc, int index)
        {
            if (this.InvokeRequired)
            {
                SetListItemImage_Callback d = new SetListItemImage_Callback(SetListItemImage);
                this.Invoke(d, new object[] { fc, index });
            }
            else
            {
                FeatureClassItem fcItem = FindFeatureClassItem(fc);
                if (fcItem == null) return;

                fcItem.ImageIndex = index;
            }
        }

        private delegate void SetItemText_Callback(IFeatureClass fc, string msg, bool append, bool nl);
        private void SetItemText(IFeatureClass fc, string msg, bool append, bool nl)
        {
            if (this.InvokeRequired)
            {
                SetItemText_Callback d = new SetItemText_Callback(SetItemText);
                this.Invoke(d, new object[] { fc, msg, append, nl });
            }
            else
            {
                FeatureClassItem fcItem = FindFeatureClassItem(fc);
                if (fcItem == null) return;

                if (append)
                    fcItem.AppendMessageLine(msg, nl);
                else
                    fcItem.MessageString = msg;

                txtMessage.Text = fcItem.MessageString;
                txtMessage.Select(fcItem.MessageString.Length, 0);
                txtMessage.ScrollToCaret();
            }
        }

        private delegate void ThreadFinished_Callback();
        private void ThreadFinished()
        {
            if (this.InvokeRequired)
            {
                ThreadFinished_Callback d = new ThreadFinished_Callback(ThreadFinished);
                this.Invoke(d);
            }
            else
            {
                if (lstFeatureclasses.SelectedItems.Count == 1)
                {
                    txtMessage.Text = ((FeatureClassItem)lstFeatureclasses.SelectedItems[0]).MessageString;
                }
                else
                {
                    txtMessage.Text = String.Empty;
                }
                btnCancel.Enabled = btnSync.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        #region Helper
        private FeatureClassItem FindFeatureClassItem(IFeatureClass fc)
        {
            if (this.InvokeRequired)
            {
                return null;
            }
            else
            {
                foreach (FeatureClassItem item in lstFeatureclasses.Items)
                {
                    if (item != null && item.FeatureClass == fc)
                        return item;
                }
            }
            return null;
        }
        #endregion

        #region ItemClasses
        private class DatasetItem
        {
            private string _name, _connectionString;
            private Guid _pluginGuid;
            private IFeatureDataset _dataset;

            public DatasetItem(string name, Guid pluginGuid, string connectionString)
            {
                _name = name;
                _pluginGuid = pluginGuid;
                _connectionString = connectionString;
            }

            public string Name
            {
                get { return _name; }
            }
            public string ConnectionString
            {
                get { return _connectionString; }
            }
            public Guid PlugInGuid
            {
                get { return _pluginGuid; }
            }
            public IFeatureDataset Dataset
            {
                get
                {
                    if (_dataset == null)
                    {
                        _dataset = PlugInManager.Create(_pluginGuid) as IFeatureDataset;
                        if (_dataset == null) return null;

                        _dataset.ConnectionString = _connectionString;
                    }
                    if (_dataset.State != DatasetState.opened)
                        _dataset.Open();

                    return _dataset;
                }
            }

            public override string ToString()
            {
                return _name;
            }
        }

        private class FeatureClassItem : ListViewItem
        {
            private IFeatureClass _fc;
            private StringBuilder _sb = new StringBuilder();

            public FeatureClassItem(IFeatureClass fc)
            {
                _fc = fc;
                if (_fc == null) return;

                base.Text = _fc.Name;
                base.ImageIndex = 0;
                base.Checked = true;
            }

            public IFeatureClass FeatureClass
            {
                get { return _fc; }
            }

            public string MessageString
            {
                get { return _sb.ToString(); }
                set { _sb = new StringBuilder(value); }
            }
            public void AppendMessageLine(string msg, bool appendNL)
            {
                _sb.Append(msg);
                if (appendNL) _sb.Append("\r\n");
            }
        }
        #endregion 

        #region SplashSreen
        private void SplashScreen(object splash)
        {
            if (splash is Form)
                ((Form)splash).ShowDialog();
        }

        private delegate void SleeperCallback(object splash);
        private void Sleeper(object splash)
        {
            if (this.InvokeRequired)
            {
                SleeperCallback d = new SleeperCallback(Sleeper);
                this.BeginInvoke(d, new object[] { splash });
            }
            else
            {
                Thread.Sleep(1000);
                ((Form)splash).Close();
            }
        }
        #endregion
    }
}