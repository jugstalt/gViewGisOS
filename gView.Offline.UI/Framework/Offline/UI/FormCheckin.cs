using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Offline;
using gView.Framework.Data;
using gView.Framework.system;
using System.IO;

namespace gView.Framework.Offline.UI
{
    public partial class FormCheckin : Form, IFeatureClassDialog
    {
        private enum CheckinMode { CheckIn = 1, Reconcile = 2 }
        
        private IFeatureClass _childFc = null, _parentFc = null;
        private BackgroundWorker _worker;

        public FormCheckin()
        {
            InitializeComponent();
        }

        #region IFeatureClassDialog Member

        public void ShowDialog(gView.Framework.Data.IFeatureClass fc)
        {
            _childFc = fc;
            string errMsg;
            _parentFc = Replication.FeatureClassParentFc(_childFc, out errMsg);
            if (_parentFc == null || _parentFc.Dataset == null)
            {
                MessageBox.Show("ERROR: " + errMsg);
            }
            else
            {
                txtDatasetName.Text = _parentFc.Dataset.GetType().ToString();
                txtDatasetLocation.Text = _parentFc.Dataset.ConnectionString;
                txtTargetClass.Text = _parentFc.Name;

                this.ShowDialog();
            }
        }

        #endregion

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (_childFc == null || _parentFc == null) return;

            switch (radioCheckin.Checked)
            {
                case true:
                    panelCheckin.Visible = true;
                    panelReconcile.Visible = false;
                    break;
                case false:
                    panelCheckin.Visible = false;
                    panelReconcile.Visible = true;
                    break;
            }

            btnCancel.Enabled = false;
            progressDisk1.Visible = true;
            progressDisk1.Start(90);

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerAsync(
                (radioCheckin.Checked) ? CheckinMode.CheckIn : CheckinMode.Reconcile);

            this.Close();
        }

        private int _labelIndex = 1;
        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string errMsg;
            Replication repl = new Replication();
            repl.CheckIn_FeatureInserted += new Replication.CheckIn_FeatureInsertedEventHandler(repl_CheckIn_FeatureInserted);
            repl.CheckIn_FeatureUpdated += new Replication.CheckIn_FeatureUpdatedEventHandler(repl_CheckIn_FeatureUpdated);
            repl.CheckIn_FeatureDeleted += new Replication.CheckIn_FeatureDeletedEventHandler(repl_CheckIn_FeatureDeleted);
            repl.CheckIn_ConflictDetected += new Replication.CheckIn_ConflictDetectedEventHandler(repl_CheckIn_ConflictDetected);
            repl.CheckIn_BeginPost += new Replication.CheckIn_BeginPostEventHandler(repl_CheckIn_BeginPost);
            _labelIndex = 1;

            Replication.ProcessType type =
                ((CheckinMode)e.Argument == CheckinMode.Reconcile) ?
                    Replication.ProcessType.Reconcile :
                    Replication.ProcessType.CheckinAndRelease;
            
            if (!repl.Process(_parentFc, _childFc, type, out errMsg))
            {
                MessageBox.Show(errMsg);
            }
           
            _worker = null;
            CheckInFinisched();
        }

        void repl_CheckIn_BeginPost(Replication sender)
        {
            _labelIndex = 2;
        }

        private delegate void repl_responseCallback(Replication sender, int count);
        void repl_CheckIn_FeatureDeleted(Replication sender, int count_deleted)
        {
            if (this.InvokeRequired)
            {
                repl_responseCallback d = new repl_responseCallback(repl_CheckIn_FeatureDeleted);
                this.Invoke(d, new object[] { sender, count_deleted });
            }
            else
            {
                if (panelCheckin.Visible)
                {
                    lblCounterDelete.Text = count_deleted + " Feature(s) deleted...";
                }
                if (panelReconcile.Visible)
                {
                    switch (_labelIndex)
                    {
                        case 1:
                            lblCounterDeleted1.Text = count_deleted + " ->";
                            break;
                        case 2:
                            lblCounterDeleted2.Text = "<- " + count_deleted;
                            break;
                    }
                }
            }
        }
        void repl_CheckIn_FeatureUpdated(Replication sender, int count_updated)
        {
            if (this.InvokeRequired)
            {
                repl_responseCallback d = new repl_responseCallback(repl_CheckIn_FeatureUpdated);
                this.Invoke(d, new object[] { sender, count_updated });
            }
            else
            {
                if (panelCheckin.Visible)
                {
                    lblCounterUpdate.Text = count_updated + " Feature(s) updated...";
                }
                if (panelReconcile.Visible)
                {
                    switch (_labelIndex)
                    {
                        case 1:
                            lblCounterUpdate1.Text = count_updated + " ->";
                            break;
                        case 2:
                            lblCounterUpdate2.Text = "<- " + count_updated;
                            break;
                    }
                }
            }
        }
        void repl_CheckIn_FeatureInserted(Replication sender, int count_inserted)
        {
            if (this.InvokeRequired)
            {
                repl_responseCallback d = new repl_responseCallback(repl_CheckIn_FeatureInserted);
                this.Invoke(d, new object[] { sender, count_inserted });
            }
            else
            {
                if (panelCheckin.Visible)
                {
                    lblCounterInsert.Text = count_inserted + " Feature(s) inserted...";
                } 
                if (panelReconcile.Visible)
                {
                    switch (_labelIndex)
                    {
                        case 1:
                            lblCounterInsert1.Text = count_inserted + " ->";
                            break;
                        case 2:
                            lblCounterInsert2.Text = "<- " + count_inserted;
                            break;
                    }
                }
            }
        }
        void repl_CheckIn_ConflictDetected(Replication sender, int count_confilicts)
        {
            if (this.InvokeRequired)
            {
                repl_responseCallback d = new repl_responseCallback(repl_CheckIn_ConflictDetected);
                this.Invoke(d, new object[] { sender, count_confilicts });
            }
            else
            {
                if (panelCheckin.Visible)
                {
                    lblCounterConflicts.Text = count_confilicts + " Confilicts(s) detected...";
                }
                if (panelReconcile.Visible)
                {
                    lblCounterConflicts1.Text = count_confilicts + " Confilicts(s) detected...";
                }
            }
        }

        private delegate void CheckInFinischedCallback();
        private void CheckInFinisched()
        {
            if (this.InvokeRequired)
            {
                CheckInFinischedCallback d = new CheckInFinischedCallback(CheckInFinisched);
                this.Invoke(d);
            }
            else
            {
                btnCancel.Text = "Close";
                btnCancel.Enabled = true;
                btnCheckout.Visible = false;

                progressDisk1.Visible = false;
            }
        }
        private void FormCheckin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_worker != null)
                e.Cancel = true;
        }

        private void btnScript_Click(object sender, EventArgs e)
        {
            if (_childFc == null || _childFc.Dataset == null ||
                _parentFc == null || _parentFc.Dataset == null) return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Explort Script...";
            dlg.Filter = "BATCH File(*.bat)|*.bat";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("echo off\r\n");

                sb.Append("\"%GVIEW4_HOME%\\checkin\" -source_connstr \"" + _childFc.Dataset.ConnectionString + "\" -source_guid \"" + PlugInManager.PlugInID(_childFc.Dataset) + "\" -source_fc \"" + _childFc.Name + "\" ");
                sb.Append("-dest_connstr \"" + _parentFc.Dataset.ConnectionString + "\" -dest_guid \"" + PlugInManager.PlugInID(_parentFc.Dataset) + "\" -dest_fc \"" + _parentFc.Name + "\"");
                if (radioReconcile.Checked == true)
                {
                    sb.Append(" -reconcile");
                }
                sb.Append("\r\n");

                StreamWriter sw = new StreamWriter(dlg.FileName, false);
                if (!String2DOS(sw.BaseStream, sb.ToString()))
                {
                    MessageBox.Show("Warning: Can't find encoding codepage (ibm850)...");
                    sw.WriteLine(sb.ToString());
                }
                sw.Close();
            }
        }

        private bool String2DOS(Stream stream, string str)
        {
            Encoding encoding = null;
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage == 850)   // ibm850...Westeuropäisch(DOS)
                {
                    encoding = ei.GetEncoding();
                }
            }

            if (encoding == null) return false;

            byte[] bytes = encoding.GetBytes(str);
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(bytes);

            return true;
        }
    }
}