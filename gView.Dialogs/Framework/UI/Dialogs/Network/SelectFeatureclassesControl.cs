using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Framework.UI.Dialogs.Network
{
    public partial class SelectFeatureclassesControl : UserControl
    {
        private IFeatureDataset _dataset;

        public SelectFeatureclassesControl(IFeatureDataset dataset)
        {
            InitializeComponent();
            _dataset = dataset;
        }

        #region Events
        private void SelectFeatureclassesControl_Load(object sender, EventArgs e)
        {
            if (_dataset != null)
            {
                foreach (IDatasetElement element in _dataset.Elements)
                {
                    if (element == null || !(element.Class is IFeatureClass))
                        continue;

                    IFeatureClass fc = (IFeatureClass)element.Class;
                    if (fc.GeometryType == geometryType.Polyline)
                    {
                        lstEdges.Items.Add(new FcListViewItem(fc));
                    }
                    else if (fc.GeometryType == geometryType.Point)
                    {
                        lstNodes.Items.Add(new FcListViewItem(fc));
                    }
                }
            }
        }
        #endregion

        #region Properties
        public string NetworkName
        {
            get { return txtNetworkName.Text; }
        }
        public List<IFeatureClass> EdgeFeatureclasses
        {
            get
            {
                List<IFeatureClass> fcs = new List<IFeatureClass>();
                foreach (FcListViewItem item in lstEdges.CheckedItems)
                {
                    fcs.Add(item.Featureclass);
                }
                return fcs;
            }
        }
        public List<IFeatureClass> NodeFeatureclasses
        {
            get
            {
                List<IFeatureClass> fcs = new List<IFeatureClass>();
                foreach (FcListViewItem item in lstNodes.CheckedItems)
                {
                    fcs.Add(item.Featureclass);
                }
                return fcs;
            }
        }
        #endregion

        #region ItemClasses
        internal class FcListViewItem : ListViewItem
        {
            private IFeatureClass _fc;

            public FcListViewItem(IFeatureClass fc)
            {
                _fc = fc;

                base.Text = _fc.Name;
            }

            public IFeatureClass Featureclass
            {
                get { return _fc; }
            }
        }
        #endregion
    }
}
