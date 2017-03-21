using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.Data;
using gView.Framework.GeoProcessing.ActivityBase;

namespace gView.Framework.GeoProcessing.UI.ActivityBase
{
    public partial class MergeFeaturesControl : UserControl, IPlugInParameter
    {
        private Merger _merger = null;
        private IFeatureClass _fc = null;

        public MergeFeaturesControl()
        {
            InitializeComponent();
        }

        public IField MergeField
        {
            get
            {
                FieldItem item = cmbField.SelectedItem as FieldItem;
                if (item == null) return null;

                return item.Field;
            }
        }

        public object[] Values
        {
            get
            {
                List<object> values = new List<object>();

                foreach (ListViewItem item in lstValues.CheckedItems)
                {
                    values.Add(item.Text);
                }

                return values.ToArray();
            }
        }

        #region IPlugInParameter Member

        public object Parameter
        {
            get
            {
                return _merger;
            }
            set
            {
                if (_merger != null)
                    _merger.DirtyEvent -= new EventHandler(_merger_DirtyEvent);

                _merger = value as Merger;

                if (_merger != null)
                    _merger.DirtyEvent += new EventHandler(_merger_DirtyEvent);

                MakeGUI();
            }
        }

        #endregion

        #region ItemClasses
        private class FieldItem
        {
            private IField _field;

            public FieldItem(IField field)
            {
                _field = field;
            }

            public IField Field
            {
                get { return _field; }
            }

            public override string ToString()
            {
                if (_field != null)
                    return _field.name;

                return "No Field";
            }
        }
        #endregion

        void _merger_DirtyEvent(object sender, EventArgs e)
        {
            MakeGUI();
        }

        void MakeGUI()
        {
            if (_merger == null) return;

            if (_merger.Sources.Count == 1 &&
                _merger.Sources[0].Data is IDatasetElement)
            {
                IFeatureClass fc = ((IDatasetElement)_merger.Sources[0].Data).Class as IFeatureClass;
                if (fc != _fc)
                {
                    _fc = fc;
                }
                else
                {
                    return;
                }
            }

            cmbField.Items.Clear();
            if (_fc == null) return;

            txtSource.Text = _fc.Name + ":";
            cmbField.Items.Add(new FieldItem(null));

            if (_fc.Fields == null) return;
            foreach (IField field in _fc.Fields)
            {
                if (field.type == FieldType.ID ||
                    field.type == FieldType.Shape ||
                    field.type == FieldType.binary ||
                    field.type == FieldType.unknown) continue;

                cmbField.Items.Add(new FieldItem(field));
            }

            cmbField.SelectedIndex = 0;
        }

        void FillValueList()
        {
            lstValues.Items.Clear();

            if (_fc == null ||
                _merger == null ||
                _merger.Sources.Count != 1 ||
                ((FieldItem)cmbField.SelectedItem).Field == null)
            {
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            IActivityData aData = _merger.Sources[0];
            if (aData.QueryMethod == QueryMethod.All ||
                aData.QueryMethod == QueryMethod.Filter)
            {
                DistinctFilter filter = new DistinctFilter(((FieldItem)cmbField.SelectedItem).Field.name);
                if (aData.QueryMethod == QueryMethod.Filter)
                {
                    filter.WhereClause = aData.FilterClause;
                }
                filter.OrderBy = ((FieldItem)cmbField.SelectedItem).Field.name;

                using (IFeatureCursor cursor = _fc.GetFeatures(filter))
                {
                    IFeature feature;
                    while ((feature = cursor.NextFeature) != null)
                    {
                        ListViewItem item = new ListViewItem(feature[0].ToString(), 0);
                        item.Checked = true;
                        lstValues.Items.Add(item);
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void cmbField_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillValueList();

            if (_merger != null)
            {
                _merger.MergeField = this.MergeField;
                _merger.Values = this.Values;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            FillValueList();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lstValues.Items)
                item.Checked = true;
        }

        private void btnUnselectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lstValues.Items)
                item.Checked = true;
        }

        private void lstValues_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_merger != null)
            {
                _merger.Values = this.Values;
            }
        }
    }
}
