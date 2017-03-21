using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Network;

namespace gView.Framework.UI.Dialogs.Network
{
    public partial class FormNewNetworkclass : Form
    {
        private IFeatureDataset _dataset;
        private SelectFeatureclassesControl _selecteFeturesclasses;
        private NetworkToleranceControl _tolerance;
        private ComplexEdgesControl _complexEdges;
        private NetworkEdgeWeightsControl _edgeWeights;
        private NetworkSwitchesControl _switches;

        public FormNewNetworkclass(IFeatureDataset dataset)
        {
            InitializeComponent();
            _dataset = dataset;

            _selecteFeturesclasses = new SelectFeatureclassesControl(_dataset);
            _tolerance = new NetworkToleranceControl();
            _complexEdges = new ComplexEdgesControl(dataset, _selecteFeturesclasses);
            _switches = new NetworkSwitchesControl(dataset, _selecteFeturesclasses);
            _edgeWeights = new NetworkEdgeWeightsControl(dataset, _selecteFeturesclasses);

            wizardControl1.AddPage(_selecteFeturesclasses);
            wizardControl1.AddPage(_tolerance);
            wizardControl1.AddPage(_complexEdges);
            wizardControl1.AddPage(_switches);
            wizardControl1.AddPage(_edgeWeights);
        }

        private void FormNewNetworkclass_Load(object sender, EventArgs e)
        {
            wizardControl1.Init();
        }

        #region Properties
        public string NetworkName
        {
            get { return _selecteFeturesclasses.NetworkName; }
        }
        public List<IFeatureClass> EdgeFeatureclasses
        {
            get
            {
                return _selecteFeturesclasses.EdgeFeatureclasses;
            }
        }
        public List<IFeatureClass> NodeFeatureclasses
        {
            get
            {
                return _selecteFeturesclasses.NodeFeatureclasses;
            }
        }
        public double SnapTolerance
        {
            get { return _tolerance.Tolerance; }
        }
        public List<int> ComplexEdgeFcIds
        { 
            get { return _complexEdges.ComplexEdgeFcIds; }
        }
        public GraphWeights GraphWeights
        {
            get { return _edgeWeights.GraphWeights; }
        }
        public Dictionary<int, string> SwitchNodeFcIds
        {
            get
            {
                return _switches.SwitchNodeFcIds;
            }
        }
        public Dictionary<int, NetworkNodeType> NetworkNodeTypeFcIds
        {
            get
            {
                return _switches.NetworkNodeTypeFcIds;
            }
        }
        #endregion
    }
}
