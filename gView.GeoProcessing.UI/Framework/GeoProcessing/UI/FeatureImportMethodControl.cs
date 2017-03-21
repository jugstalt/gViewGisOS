using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.Framework.GeoProcessing.UI
{
    public partial class FeatureImportMethodControl : UserControl
    {
        private IDatasetElement _element = null;

        #region MyEvents
        public event EventHandler QueryMethodChanged = null;
        public event EventHandler FilterClauseChanged = null;
        #endregion

        public FeatureImportMethodControl()
        {
            InitializeComponent();

            btnAllFeatures.Checked = true;
        }

        public IDatasetElement DatasetElement
        {
            get
            {
                return _element;
            }
            set
            {
                if (_element == value &&
                    _element != null) return;

                _element = value;
                this.Enabled = (_element != null);

                if (_element != null)
                {
                    lblSelection.Visible =
                    btnSelectedFeatures.Enabled =
                        (_element is IFeatureSelection &&
                        ((IFeatureSelection)_element).SelectionSet != null &&
                        ((IFeatureSelection)_element).SelectionSet.Count > 0);

                    if (lblSelection.Visible)
                        lblSelection.Text = ((IFeatureSelection)_element).SelectionSet.Count == 1 ?
                            "(1 Feature)" :
                            "(" + ((IFeatureSelection)_element).SelectionSet.Count + " Features)";

                    btnFilterFeatures.Enabled = _element.Class is ITableClass;

                    filterFeaturesControl1.TableClass = _element.Class as ITableClass;
                }
            }
        }

        public QueryMethod QueryMethod
        {
            get
            {
                if (btnAllFeatures.Checked)
                    return QueryMethod.All;
                if (btnSelectedFeatures.Checked)
                    return QueryMethod.Selected;

                return QueryMethod.Filter;
            }
            set
            {
                switch (value)
                {
                    case QueryMethod.All:
                        btnAllFeatures.Checked = true;
                        break;
                    case QueryMethod.Selected:
                        btnSelectedFeatures.Checked = true;
                        break;
                    case QueryMethod.Filter:
                        btnFilterFeatures.Checked = true;
                        break;
                }
            }
        }

        public string FilterClause
        {
            get { return filterFeaturesControl1.FilterClause; }
            set { filterFeaturesControl1.FilterClause = value; }
        }

        #region Events
        private void filterFeaturesControl1_FilterClauseChanged(object sender, EventArgs e)
        {
            if (FilterClauseChanged != null)
                FilterClauseChanged(this, new EventArgs());
        }

        private void btnAllFeatures_CheckedChanged(object sender, EventArgs e)
        {
            if (QueryMethodChanged != null)
                QueryMethodChanged(this, new EventArgs());
        }

        private void btnSelectedFeatures_CheckedChanged(object sender, EventArgs e)
        {
            if (QueryMethodChanged != null)
                QueryMethodChanged(this, new EventArgs());
        }

        private void btnFilterFeatures_CheckedChanged(object sender, EventArgs e)
        {
            if (QueryMethodChanged != null)
                QueryMethodChanged(this, new EventArgs());
        }
        #endregion
    }
}