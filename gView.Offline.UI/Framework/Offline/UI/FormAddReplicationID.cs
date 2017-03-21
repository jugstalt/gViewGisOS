using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Offline;

namespace gView.Framework.Offline.UI
{
    public partial class FormAddReplicationID : Form,IFeatureClassDialog
    {
        private IFeatureClass _fc;
        private IFeatureDatabaseReplication _fdb;
        private BackgroundWorker _worker;

        public FormAddReplicationID()
        {
            InitializeComponent();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            btnCreate.Enabled = btnCancel.Enabled = false;
            progressDisk1.Visible = lblCounter.Visible = true;
            progressDisk1.Start(90);

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerAsync(txtFieldname.Text);

            Cursor = Cursors.WaitCursor;
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string errMsg;
            Replication repl = new Replication();
            repl.ReplicationGuidsAppended += new Replication.ReplicationGuidsAppendedEventHandler(Replication_ReplicationGuidsAppended);
            
            if (!repl.AppendReplicationIDField(_fdb, _fc, e.Argument.ToString(), out errMsg))
            {
                MessageBox.Show("Error: " + errMsg);
            }
            _worker = null;
            CloseWindow();
        }

        private delegate void CloseWindowCallback();
        private void CloseWindow()
        {
            if (this.InvokeRequired)
            {
                CloseWindowCallback callback = new CloseWindowCallback(CloseWindow);
                this.Invoke(callback);
            }
            else
            {
                this.Close();
            }
        }
        void Replication_ReplicationGuidsAppended(Replication sender, int count_appended)
        {
            if (this.InvokeRequired)
            {
                Replication.ReplicationGuidsAppendedEventHandler callback = new Replication.ReplicationGuidsAppendedEventHandler(Replication_ReplicationGuidsAppended);
                this.Invoke(callback, new object[] { sender, count_appended });
            }
            else
            {
                lblCounter.Text = count_appended.ToString() + " Features updated...";
            }
        }

        private void FormAddReplicationID_Load(object sender, EventArgs e)
        {
            if (_fc == null || _fdb == null) this.Close();
        }

        private void FormAddReplicationID_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_worker != null)
                e.Cancel = true;
        }

        #region IFeatureClassDialog Member

        public void ShowDialog(IFeatureClass fc)
        {
            _fc = fc;
            if (_fc != null && fc.Dataset != null)
            {
                _fdb = _fc.Dataset.Database as IFeatureDatabaseReplication;

                this.ShowDialog();
            }
        }

        #endregion
    }
}