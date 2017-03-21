using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;

namespace gView.Framework.GeoProcessing.UI.ActivityBase
{
    public partial class BufferControl : UserControl,IPlugInParameter
    {
        private gView.Framework.GeoProcessing.ActivityBase.Buffer _buffer = null;
        public BufferControl()
        {
            InitializeComponent();
        }

        #region IPlugInParameter Member

        public object Parameter
        {
            get
            {
                return _buffer;
            }
            set
            {
                _buffer = value as gView.Framework.GeoProcessing.ActivityBase.Buffer;

                if (_buffer != null)
                    txtDouble.Double = _buffer.BufferDistance;
            }
        }

        #endregion

        private void txtDouble_TextChanged(object sender, EventArgs e)
        {
            if (_buffer != null)
                _buffer.BufferDistance = txtDouble.Double;
        }

        
    }
}
