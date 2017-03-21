using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;

namespace gView.Plugins.GPS.Plugins.GPS
{
    [gView.Framework.system.RegisterPlugIn("602fa4fb-b10d-4eee-b2f5-a789cd6a044e")]
    public class GpsRibbonTab : ICartoRibbonTab
    {
        private List<RibbonGroupBox> _groups;

        public GpsRibbonTab()
        {
            _groups = new List<RibbonGroupBox>(
                new RibbonGroupBox[]{
                    new RibbonGroupBox(String.Empty,
                        new RibbonItem[] {
                            new RibbonItem(new Guid("05C896A0-090A-488b-93D2-817DDD6FF04F")),   // Customize 
                            new RibbonItem(new Guid("9617B354-6757-4c82-8B0F-F7987D7587D5")),   // Zoom 2 Position
                            new RibbonItem(new Guid("74BCAB24-38FB-4600-B37C-64A7AE3AD702"))  // Tracking
                        }
                        )
                }
                );
        }

        #region ICartoRibbonTab Member

        public string Header
        {
            get { return "GPS"; }
        }

        public List<RibbonGroupBox> Groups
        {
            get { return _groups; }
        }

        public int SortOrder
        {
            get { return 400; }
        }

        public bool IsVisible(IMapDocument mapDocument)
        {
            if (mapDocument == null || mapDocument.FocusMap == null)
                return false;

            return true;
        }
        #endregion
    }
}
