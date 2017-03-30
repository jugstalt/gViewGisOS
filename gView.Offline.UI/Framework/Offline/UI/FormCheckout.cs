using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.UI;
using gView.Framework.Offline;
using gView.Framework.system;
using gView.Framework.system.UI;
using System.Threading;
using gView.Framework.UI.Dialogs;
using System.IO;
using gView.Framework.UI.Controls.Filter;
using gView.DataSources.Fdb.UI;
using gView.DataSources.Fdb.MSAccess;

namespace gView.Framework.Offline.UI
{
    public partial class FormCheckout : Form,IFeatureClassDialog
    {
        private IFeatureClass _sourceFC;
        private IFeatureDatabaseReplication _sourceFDB;

        public FormCheckout()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenFeatureDatasetFilter());

            ExplorerDialog exDlg = new ExplorerDialog("New Target Featureclass",
                SaveFeatureClassFilters.DatabaseFilters,
                false);

            if (exDlg.ShowDialog() == DialogResult.OK &&
                exDlg.ExplorerObjects.Count == 1)
            {
                IExplorerObject parentObject = exDlg.ExplorerObjects[0];

                if (parentObject.Object is IFeatureDataset &&
                    ((IDataset)parentObject.Object).Database is IFeatureUpdater)
                {
                    _dataset = (IFeatureDataset)parentObject.Object;
                }
                else if (exDlg.SelectedExplorerDialogFilter.FilterObject is IFeatureDataset &&
                    ((IDataset)exDlg.SelectedExplorerDialogFilter.FilterObject).Database is IFileFeatureDatabase)
                {
                    IFileFeatureDatabase fileDB = (IFileFeatureDatabase)((IFeatureDataset)exDlg.SelectedExplorerDialogFilter.FilterObject).Database;

                    _dataset = fileDB[parentObject.FullName];
                }
                else
                {
                    MessageBox.Show("Can't determine target featureclass!");
                    return;
                }

                if (!(_dataset.Database is IFeatureDatabaseReplication))
                {
                    MessageBox.Show("Can't checkout to this database...");
                    return;
                }
                txtDatasetName.Text = _dataset.Database.ToString();
                txtDatasetLocation.Text = parentObject.FullName;
                txtTargetClass.Text = exDlg.TargetName;
            }
            btnCheckout.Enabled = btnScript.Enabled = exDlg.TargetName != String.Empty;
        }

        #region IFeatureClassDialog Member

        public void ShowDialog(IFeatureClass fc)
        {
            _sourceFC = fc;
            if (_sourceFC != null && fc.Dataset != null)
            {
                _sourceFDB = _sourceFC.Dataset.Database as IFeatureDatabaseReplication;

                this.ShowDialog();
            }
        }

        #endregion

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            Replication.VersionRights rights = Replication.VersionRights.NONE;
            Replication.ConflictHandling ch = Replication.ConflictHandling.NORMAL;

            if (chkINSERT.Checked) rights |= Replication.VersionRights.INSERT;
            if (chkUPDATE.Checked) rights |= Replication.VersionRights.UPDATE;
            if (chkDELETE.Checked) rights |= Replication.VersionRights.DELETE;

            if (radioNoConflict.Checked) ch = Replication.ConflictHandling.NONE;
            else if (radioParentWins.Checked) ch = Replication.ConflictHandling.PARENT_WINS;
            else if (radioChildWins.Checked) ch = Replication.ConflictHandling.CHILD_WINS;
            else if (radioNewerWins.Checked) ch = Replication.ConflictHandling.NEWER_WINS;

            string replIDField = Replication.FeatureClassReplicationIDFieldname(_sourceFC);
            if (String.IsNullOrEmpty(replIDField))
            {
                MessageBox.Show("ERROR: can't determine replication id fieldname from source featureclass...");
                return;
            }
            _targetName = txtTargetClass.Text;
            ExportDatasetObject(_sourceFC);

            IDatasetElement element=_dataset[_targetName];
            if (element == null)
            {
                MessageBox.Show("ERROR: Target "+_targetName+" not found...");
                return;
            }
            IFeatureClass destFC = element.Class as IFeatureClass;

            string errMsg;
            List<Guid> checkout_guids = Replication.FeatureClassSessions(destFC);
            if (checkout_guids != null && checkout_guids.Count != 0)
            {
                errMsg = "Can't check out to this featureclass\n";
                errMsg += "Check in the following Sessions first:\n";
                foreach (Guid g in checkout_guids)
                    errMsg += "   CHECKOUT_GUID: " + g.ToString();
                MessageBox.Show("ERROR:\n" + errMsg);
                return;
            }
            if (!Replication.InsertReplicationIDFieldname(destFC, replIDField, out errMsg))
            {
                MessageBox.Show("ERROR: " + errMsg);
                return;
            }

            if (!Replication.InsertNewCheckoutSession(
                _sourceFC,
                Replication.VersionRights.INSERT|Replication.VersionRights.UPDATE|Replication.VersionRights.DELETE,
                destFC,
                rights,
                ch,
                txtCheckoutDescription.Text, 
                out errMsg))
            {
                MessageBox.Show("ERROR: " + errMsg);
                return;
            }

            if (!Replication.InsertCheckoutLocks(_sourceFC, destFC, out errMsg))
            {
                MessageBox.Show("ERROR: " + errMsg);
                return;
            }
            this.Close();
        }

        #region Export
        private FeatureImport _export = null;
        private FDBImport _fdbExport = null;
        private IFeatureDataset _dataset = null;
        private IQueryFilter _filter = null;
        private string _targetName = String.Empty;

        private void ExportDatasetObject(object datasetObject)
        {
            if (_dataset.Database is AccessFDB)
            {
                ExportDatasetObject_fdb(datasetObject);
            }
            else
            {
                ExportDatasetObject_db(datasetObject);
            }
        }

        #region FDB
        private void ExportDatasetObject_fdb(object datasetObject)
        {
            if (datasetObject is IFeatureDataset)
            {
                IFeatureDataset dataset = (IFeatureDataset)datasetObject;
                foreach (IDatasetElement element in dataset.Elements)
                {
                    if (element is IFeatureLayer)
                    {
                        ExportDatasetObject(((IFeatureLayer)element).FeatureClass);
                    }
                }
            }
            if (datasetObject is IFeatureClass)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ExportAsync_fdb));

                if (_fdbExport == null)
                    _fdbExport = new FDBImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }

                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_fdbExport, (IFeatureClass)datasetObject);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Export Features: " + ((IFeatureClass)datasetObject).Name;
                progress.ShowDialog();
                _fdbExport = null;
            }
            if (datasetObject is FeatureClassListViewItem)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ExportAsync_fdb));

                if (_fdbExport == null)
                    _fdbExport = new FDBImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }

                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_fdbExport, ((FeatureClassListViewItem)datasetObject).FeatureClass);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Export Features: " + ((FeatureClassListViewItem)datasetObject).TargetName;
                progress.ShowDialog();
                _fdbExport = null;
            }
        }
        private void ExportAsync_fdb(object element)
        {
            if (_fdbExport == null) return;

            List<IQueryFilter> filters = null;
            if (_filter != null)
            {
                filters = new List<IQueryFilter>();
                filters.Add(_filter);
            }

            if (element is IFeatureClass)
            {
                IFeatureClass fc = (IFeatureClass)element;
                ISpatialIndexDef def = null;
                if (fc.Dataset != null && fc.Dataset.Database is AccessFDB)
                {
                    def = ((AccessFDB)fc.Dataset.Database).FcSpatialIndexDef(fc.Name);
                    if (_dataset.Database is AccessFDB)
                    {
                        ISpatialIndexDef dsDef = ((AccessFDB)_dataset.Database).SpatialIndexDef(_dataset.DatasetName);
                        if (dsDef.GeometryType != def.GeometryType)
                            def = dsDef;
                    }
                }
                if (!_fdbExport.ImportToNewFeatureclass(
                    _dataset.Database as IFeatureDatabase,
                    _dataset.DatasetName,
                    _targetName,
                    fc,
                    null,
                    true,
                    filters,
                    def))
                {
                    MessageBox.Show(_fdbExport.lastErrorMsg);
                }
            }
            else if (element is FeatureClassListViewItem)
            {
                FeatureClassListViewItem item = element as FeatureClassListViewItem;
                if (item.FeatureClass == null) return;

                IFeatureClass fc = item.FeatureClass;
                ISpatialIndexDef def = null;
                if (fc.Dataset != null && fc.Dataset.Database is AccessFDB)
                {
                    def = ((AccessFDB)fc.Dataset.Database).FcSpatialIndexDef(fc.Name);
                    if (_dataset.Database is AccessFDB)
                    {
                        ISpatialIndexDef dsDef = ((AccessFDB)_dataset.Database).SpatialIndexDef(_dataset.DatasetName);
                        if (dsDef.GeometryType != def.GeometryType)
                            def = dsDef;
                    }
                }

                if (!_fdbExport.ImportToNewFeatureclass(
                    _dataset.Database as IFeatureDatabase,
                    _dataset.DatasetName,
                    item.TargetName,
                    fc,
                    item.ImportFieldTranslation,
                    true,
                    filters,
                    def))
                {
                    MessageBox.Show(_fdbExport.lastErrorMsg);
                }
            }
        }
        #endregion

        #region All other DBs
        private void ExportDatasetObject_db(object datasetObject)
        {
            if (datasetObject is IFeatureDataset)
            {
                IFeatureDataset dataset = (IFeatureDataset)datasetObject;
                foreach (IDatasetElement element in dataset.Elements)
                {
                    if (element is IFeatureLayer)
                    {
                        ExportDatasetObject(((IFeatureLayer)element).FeatureClass);
                    }
                }
            }
            if (datasetObject is IFeatureClass)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ExportAsync_db));

                if (_export == null)
                    _export = new FeatureImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }

                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_export, (IFeatureClass)datasetObject);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Export Features: " + ((IFeatureClass)datasetObject).Name;
                progress.ShowDialog();
                _export = null;
            }
            if (datasetObject is FeatureClassListViewItem)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ExportAsync_db));

                if (_export == null)
                    _export = new FeatureImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }

                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_export, ((FeatureClassListViewItem)datasetObject).FeatureClass);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Export Features: " + ((FeatureClassListViewItem)datasetObject).TargetName;
                progress.ShowDialog();
                _export = null;
            }
        }
        private void ExportAsync_db(object element)
        {
            if (_export == null) return;

            List<IQueryFilter> filters = null;
            if (_filter != null)
            {
                filters = new List<IQueryFilter>();
                filters.Add(_filter);
            }

            if (element is IFeatureClass)
            {
                if (!_export.ImportToNewFeatureclass(
                    _dataset,
                    ((IFeatureClass)element).Name,
                    (IFeatureClass)element,
                    null,
                    true,
                    filters))
                {
                    MessageBox.Show(_export.lastErrorMsg);
                }
            }
            else if (element is FeatureClassListViewItem)
            {
                FeatureClassListViewItem item = element as FeatureClassListViewItem;
                if (item.FeatureClass == null) return;

                if (!_export.ImportToNewFeatureclass(
                    _dataset,
                    item.TargetName,
                    item.FeatureClass,
                    item.ImportFieldTranslation,
                    true,
                    filters))
                {
                    MessageBox.Show(_export.lastErrorMsg);
                }
            }
        }
        #endregion

        class FeatureClassImportProgressReporter : IProgressReporter
        {
            private ProgressReport _report = new ProgressReport();
            private ICancelTracker _cancelTracker = null;

            public FeatureClassImportProgressReporter(object import, IFeatureClass source)
            {
                if (import == null) return;

                if (import is FDBImport)
                {
                    _cancelTracker = ((FDBImport)import).CancelTracker;

                    if (source != null) _report.featureMax = source.CountFeatures;
                    ((FDBImport)import).ReportAction += new FDBImport.ReportActionEvent(FeatureClassImportProgressReporter_ReportAction);
                    ((FDBImport)import).ReportProgress += new FDBImport.ReportProgressEvent(FeatureClassImportProgressReporter_ReportProgress);
                    ((FDBImport)import).ReportRequest += new FDBImport.ReportRequestEvent(FeatureClassImportProgressReporter_ReportRequest);
                }
                if (import is FeatureImport)
                {
                    _cancelTracker = ((FeatureImport)import).CancelTracker;

                    if (source != null) _report.featureMax = source.CountFeatures;
                    ((FeatureImport)import).ReportAction += new FeatureImport.ReportActionEvent(import_ReportAction);
                    ((FeatureImport)import).ReportProgress += new FeatureImport.ReportProgressEvent(import_ReportProgress);
                    ((FeatureImport)import).ReportRequest += new FeatureImport.ReportRequestEvent(import_ReportRequest);
                }
            }

            #region FDB
            void FeatureClassImportProgressReporter_ReportRequest(FDBImport sender, gView.DataSources.Fdb.UI.RequestArgs args)
            {
                args.Result = MessageBox.Show(
                    args.Request,
                    "Warning",
                    args.Buttons,
                    MessageBoxIcon.Warning);
            }

            void FeatureClassImportProgressReporter_ReportProgress(FDBImport sender, int progress)
            {
                if (ReportProgress == null) return;

                _report.featureMax = Math.Max(_report.featureMax, progress);
                _report.featurePos = progress;

                ReportProgress(_report);
            }

            void FeatureClassImportProgressReporter_ReportAction(FDBImport sender, string action)
            {
                if (ReportProgress == null) return;

                _report.featurePos = 0;
                _report.Message = action;

                ReportProgress(_report);
            }
            #endregion

            void import_ReportRequest(FeatureImport sender, gView.Framework.system.UI.RequestArgs args)
            {
                args.Result = MessageBox.Show(
                    args.Request,
                    "Warning",
                    args.Buttons,
                    MessageBoxIcon.Warning);
            }

            void import_ReportProgress(FeatureImport sender, int progress)
            {
                if (ReportProgress == null) return;

                _report.featureMax = Math.Max(_report.featureMax, progress);
                _report.featurePos = progress;

                ReportProgress(_report);
            }

            void import_ReportAction(FeatureImport sender, string action)
            {
                if (ReportProgress == null) return;

                _report.featurePos = 0;
                _report.Message = action;

                ReportProgress(_report);
            }

            #region IProgressReporter Member

            public event ProgressReporterEvent ReportProgress;

            public ICancelTracker CancelTracker
            {
                get { return _cancelTracker; }
            }
            #endregion
        }
        #endregion      

        private void btnScript_Click(object sender, EventArgs e)
        {
            if (_dataset == null || _sourceFC == null || _sourceFC.Dataset == null) return;

            Replication.VersionRights rights = Replication.VersionRights.NONE;

            if (chkINSERT.Checked) rights |= Replication.VersionRights.INSERT;
            if (chkUPDATE.Checked) rights |= Replication.VersionRights.UPDATE;
            if (chkDELETE.Checked) rights |= Replication.VersionRights.DELETE;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Explort Script...";
            dlg.Filter = "BATCH File(*.bat)|*.bat";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("echo off\r\n");

                sb.Append("\"%GVIEW4_HOME%\\gView.Cmd.CopyFeatureclass\" -source_connstr \"" + _sourceFC.Dataset.ConnectionString + "\" -source_guid \"" + PlugInManager.PlugInID(_sourceFC.Dataset) + "\" -source_fc \"" + _sourceFC.Name + "\" ");
                sb.Append("-dest_connstr \"" + _dataset.ConnectionString + "\" -dest_guid \"" + PlugInManager.PlugInID(_dataset) + "\" -dest_fc \"" + txtTargetClass.Text + "\" ");
                sb.Append("-checkout \"" + txtCheckoutDescription.Text + "\" ");
                sb.Append("-pr iud -cr ");
                if (rights == Replication.VersionRights.NONE) sb.Append("x");
                else
                {
                    if (Bit.Has(rights, Replication.VersionRights.INSERT)) sb.Append("i");
                    if (Bit.Has(rights, Replication.VersionRights.UPDATE)) sb.Append("u");
                    if (Bit.Has(rights, Replication.VersionRights.DELETE)) sb.Append("d");
                }
                sb.Append(" -ch ");
                if (radioNoConflict.Checked) sb.Append("none");
                else if (radioNormalConflict.Checked) sb.Append("normal");
                else if (radioParentWins.Checked) sb.Append("parent_wins");
                else if (radioChildWins.Checked) sb.Append("child_wins");
                else if (radioNewerWins.Checked) sb.Append("newer_wins");

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