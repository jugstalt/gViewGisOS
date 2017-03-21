using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;

namespace gView.Desktop.Wpf.Carto.Items
{
    public interface ICheckAbleButton
    {
        bool Checked { get; set; }
        ITool Tool { get; }
    }
}
