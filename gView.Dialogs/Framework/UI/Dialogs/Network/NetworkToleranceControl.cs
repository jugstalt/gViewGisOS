using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs.Network
{
    public partial class NetworkToleranceControl : UserControl
    {
        public NetworkToleranceControl()
        {
            InitializeComponent();
        }

        #region Properties
        public double Tolerance
        {
            get
            {
                if (chkUseSnapTolerance.Checked)
                    return (double)numSnapTolerance.Value;

                return double.Epsilon;
            }
            set
            {
                numSnapTolerance.Value = (decimal)value;
            }
        }
        #endregion
    }
}
