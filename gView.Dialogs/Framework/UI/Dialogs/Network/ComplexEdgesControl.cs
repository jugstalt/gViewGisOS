using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.UI.Controls.Wizard;

namespace gView.Framework.UI.Dialogs.Network
{
    public partial class ComplexEdgesControl : UserControl, IWizardPageNotification
    {
        private IFeatureDatabase3 _database = null;
        private SelectFeatureclassesControl _selected;

        public ComplexEdgesControl(IFeatureDataset dataset, SelectFeatureclassesControl selected)
        {
            InitializeComponent();

            if (dataset != null)
                _database = dataset.Database as IFeatureDatabase3;

            if (_database == null)
                throw new ArgumentException();

            _selected = selected;
        }

        private void chkCreateComplexEdges_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = chkCreateComplexEdges.Checked;
        }

        #region IWizardPageNotification Member

        public void OnShowWizardPage()
        {
            if (_selected != null)
            {
                List<int> fcIds = this.ComplexEdgeFcIds;
                lstEdges.Items.Clear();

                foreach (IFeatureClass fc in _selected.EdgeFeatureclasses)
                {
                    SelectFeatureclassesControl.FcListViewItem item = new SelectFeatureclassesControl.FcListViewItem(fc);
                    int fcId = _database.GetFeatureClassID(fc.Name);
                    if (fcIds.Contains(fcId))
                        item.Checked = true;

                    lstEdges.Items.Add(item);
                }
            }
        }

        #endregion

        #region Properties
        public List<int> ComplexEdgeFcIds
        {
            get
            {
                List<int> fcIds = new List<int>();

                if (chkCreateComplexEdges.Checked == true)
                {
                    foreach (SelectFeatureclassesControl.FcListViewItem item in lstEdges.Items)
                    {
                        if (item.Checked == true)
                        {
                            fcIds.Add(_database.GetFeatureClassID(item.Featureclass.Name));
                        }
                    }
                }

                return fcIds;
            }
        }
        #endregion
    }
}
