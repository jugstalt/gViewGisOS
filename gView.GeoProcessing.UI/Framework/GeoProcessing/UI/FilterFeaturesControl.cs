using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.UI.Dialogs;
using gView.Framework.Data;

namespace gView.Framework.GeoProcessing.UI
{
    public partial class FilterFeaturesControl : UserControl
    {
        private ITableClass _tc = null;

        #region MyEvents
        public event EventHandler FilterClauseChanged = null;
        #endregion

        public FilterFeaturesControl()
        {
            InitializeComponent();
        }

        public ITableClass TableClass
        {
            get { return _tc; }
            set
            {
                if (_tc == value) return;

                _tc = value;
                txtFilter.Text = String.Empty;
            }
        }

        void MakeGUI()
        {
            btnQueryBuilder.Enabled = (_tc != null);
        }

        private void btnQueryBuilder_Click(object sender, EventArgs e)
        {
            FormQueryBuilder dlg = new FormQueryBuilder(_tc);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFilter.Text = dlg.whereClause;
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            if (FilterClauseChanged != null)
                FilterClauseChanged(this, new EventArgs());
        }

        public string FilterClause
        {
            get { return txtFilter.Text; }
            set { txtFilter.Text = value; }
        }
    }
}
