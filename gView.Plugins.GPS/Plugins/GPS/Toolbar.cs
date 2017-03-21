using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.GPS;

namespace gView.Plugins.GPS
{
    internal class Device
    {
        static private GPSDevice _GPS = null;
        static private double _lon = 0.0, _lat = 0.0;

        static internal GPSDevice GPS
        {
            get { return _GPS; }
            set
            {
                if (_GPS == value) return;
                if (_GPS != null)
                {
                    _GPS.Dispose();
                }
                _GPS = value;
            }
        }
    }

    [gView.Framework.system.RegisterPlugIn("6F063966-C90F-42e6-AB40-CBBA6577964B")]
    class Toolbar : IToolbar
    {
        private bool _visible = true;
        private List<Guid> _guids;

        public Toolbar()
        {
            _guids = new List<Guid>();
            _guids.Add(new Guid("05C896A0-090A-488b-93D2-817DDD6FF04F"));  // Customize
            _guids.Add(new Guid("9617B354-6757-4c82-8B0F-F7987D7587D5"));  // Zoom 2 Position
            _guids.Add(new Guid("74BCAB24-38FB-4600-B37C-64A7AE3AD702"));  // Tracking
        }

        #region IToolbar Members

        //public bool Visible
        //{
        //    get
        //    {
        //        return _visible;
        //    }
        //    set
        //    {
        //        _visible = value;
        //    }
        //}

        public string Name
        {
            get { return "GPS"; }
        }

        public List<Guid> GUIDs
        {
            get
            {
                return _guids;
            }
            set
            {
                _guids = value;
            }
        }

        #endregion

        #region IPersistable Members

        public string PersistID
        {
            get { return ""; }
        }

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        #endregion
    }
}
