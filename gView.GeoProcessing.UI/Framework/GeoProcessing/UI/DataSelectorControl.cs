using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.GeoProcessing.ActivityBase;
using gView.Framework.UI.Controls.Filter;
using gView.Framework.UI.Dialogs;

namespace gView.Framework.GeoProcessing.UI
{
    public partial class DataSelectorControl : UserControl
    {
        private IActivityData _aData;
        private bool _open = true;

        public DataSelectorControl(IActivityData aData)
        {
            InitializeComponent();
            
            _aData = aData;
            if (_aData == null) return;

            gbDataName.Text = _aData.DisplayName;
            importMethodCtrl.DatasetElement = null;
        }

        public bool Open
        {
            get
            {
                return _open;
            }
            set
            {
                btnAdd.Image = (value) ? global::gView.GeoProcessing.UI.Properties.Resources.add_16 : global::gView.GeoProcessing.UI.Properties.Resources.save_16;
                _open = value;

                if (_open)
                {
                    importMethodCtrl.Visible = true;
                    this.Height = 240;
                }
                else
                {
                    importMethodCtrl.Visible = false;
                    this.Height = 60;
                }
            }
        }

        public void AddData(IDatasetElement element)
        {
            if (element == null || _aData == null) return;

            foreach (object item in cmbData.Items)
            {
                if (!(item is DatasetElementItem)) continue;

                if (((DatasetElementItem)item).Element == element) return;
            }

            if (_aData.ProcessAble(element))
            {
                cmbData.Items.Add(new DatasetElementItem(element));
                if (cmbData.Items.Count == 1)
                    cmbData.SelectedIndex = 0;
            }
        }

        internal void AddData(DatasetItem dsItem)
        {
            if (!_open &&
                dsItem != null &&
                dsItem.Dataset != null &&
                dsItem.TargetName != null)
            {
                cmbData.Items.Add(dsItem);

                if (dsItem != null)
                    cmbData.SelectedItem = dsItem;
            }
        }
        #region ItemClasses 
        private class DatasetElementItem
        {
            private IDatasetElement _element=null;
            private QueryMethod _queryMethode = QueryMethod.All;
            private string _filterClause = String.Empty;

            public DatasetElementItem(IDatasetElement element)
            {
                _element = element;
            }

            public IDatasetElement Element
            {
                get { return _element; }
            }
            public QueryMethod QueryMethod
            {
                get { return _queryMethode; }
                set { _queryMethode = value; }
            }
            public string FilterClause
            {
                get { return _filterClause; }
                set { _filterClause = value; }
            }

            public override string ToString()
            {
                if (_element != null && _element.Class != null)
                    return _element.Class.Name;

                return String.Empty;
            }
        }
        internal class DatasetItem
        {
            private IDataset _dataset;
            private string _targetName;

            public DatasetItem(IDataset dataset, string targetName)
            {
                _dataset = dataset;
                _targetName = targetName;
            }

            public IDataset Dataset
            {
                get { return _dataset; }
            }
            public string TargetName
            {
                get { return _targetName; }
            }
            public override string ToString()
            {
                if (_dataset != null)
                    return _dataset.DatasetName + ": " + TargetName;

                return String.Empty;
            }
        }
        #endregion

        #region Events
        private void btnAdd_Click(object sender, EventArgs e)
        {
            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            if (_open)
            {
                filters.Add(new OpenActivityDataFilter(_aData));
            }
            else
            {
                filters = SaveFeatureClassFilters.AllFilters;
            }
            ExplorerDialog dlg = new ExplorerDialog(gbDataName.Text,
                filters,
                _open);

            if (dlg.ShowDialog() == DialogResult.OK &&
                dlg.ExplorerObjects != null &&
                dlg.ExplorerObjects.Count > 0)
            {
                object item = null;
                foreach (IExplorerObject exObject in dlg.ExplorerObjects)
                {
                    if (exObject == null) continue;

                    if (_open)  // Source
                    {
                        if (exObject.Object is IDatasetElement)
                        {
                            item = new DatasetElementItem((IDatasetElement)exObject.Object);
                            cmbData.Items.Add(item);
                        }
                        else if (exObject.Object is IClass)
                        {
                            item = new DatasetElementItem(new DatasetElement((IClass)exObject.Object));
                            cmbData.Items.Add(item);
                        }
                    }
                    else  // Target
                    {
                        if (exObject.Object is IDataset &&
                            !String.IsNullOrEmpty(dlg.TargetName))
                        {
                            item = new DatasetItem((IDataset)exObject.Object, dlg.TargetName);
                            cmbData.Items.Add(item);
                        }
                        else if (dlg.SelectedExplorerDialogFilter != null &&
                            dlg.SelectedExplorerDialogFilter.FilterObject is IFeatureDataset &&
                            !String.IsNullOrEmpty(dlg.TargetName))
                        {
                            IFileFeatureDatabase fileDB = (IFileFeatureDatabase)((IFeatureDataset)dlg.SelectedExplorerDialogFilter.FilterObject).Database;
                            IFeatureDataset ds = fileDB[exObject.FullName];
                            if (ds != null)
                            {
                                item = new DatasetItem(ds, dlg.TargetName);
                                cmbData.Items.Add(item);
                            }
                        }
                    }
                }
                // letztes eingefügtes auswählen
                if (item != null)
                    cmbData.SelectedItem = item;
            }
        }
        private void cmbData_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbData.SelectedItem is DatasetItem)
            {
                _aData.Data = new TargetDatasetElement(
                    ((DatasetItem)cmbData.SelectedItem).TargetName,
                    ((DatasetItem)cmbData.SelectedItem).Dataset);
            }
            else if (cmbData.SelectedItem is DatasetElementItem)
            {
                _aData.Data = ((DatasetElementItem)cmbData.SelectedItem).Element;

                if (importMethodCtrl.Visible)
                {
                    importMethodCtrl.DatasetElement = ((DatasetElementItem)cmbData.SelectedItem).Element;
                    importMethodCtrl.QueryMethod = ((DatasetElementItem)cmbData.SelectedItem).QueryMethod;
                    importMethodCtrl.FilterClause = ((DatasetElementItem)cmbData.SelectedItem).FilterClause;
                }
            }
        }

        private void importMethodCtrl_QueryMethodChanged(object sender, EventArgs e)
        {
            DatasetElementItem item = cmbData.SelectedItem as DatasetElementItem;
            if (item != null)
            {
                item.QueryMethod = importMethodCtrl.QueryMethod;
            }
            if (_aData != null)
                _aData.QueryMethod = importMethodCtrl.QueryMethod;
        }

        private void importMethodCtrl_FilterClauseChanged(object sender, EventArgs e)
        {
            DatasetElementItem item = cmbData.SelectedItem as DatasetElementItem;
            if (item != null)
            {
                item.FilterClause = importMethodCtrl.FilterClause;
            }
            if (_aData != null)
                _aData.FilterClause = importMethodCtrl.FilterClause;
        }
        #endregion
    }
}
