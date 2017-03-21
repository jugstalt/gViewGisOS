using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.GPS.UI;
using gView.Framework.GPS.UI;
using gView.Framework.GPS;

namespace gView.Plugins.GPS
{
    internal class M
    {
        public static double DEG2RAD = 180.0 / Math.PI;
    }

    [gView.Framework.system.RegisterPlugIn("05C896A0-090A-488b-93D2-817DDD6FF04F")]
    public class GPSCustomize : ITool
    {
        IMapDocument _mapDocument = null;

        #region ITool Members

        public string Name
        {
            get { return "GPSCustomize"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return "Customize GPS"; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get
            {
                return (new Buttons()).imageList1.Images[1];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _mapDocument = (IMapDocument)hook;
        }

        public void OnEvent(object MapEvent)
        {
            FormGPS dlg = new FormGPS();
            dlg.Device = Device.GPS;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Device.GPS = dlg.Device;
            }
            if (_mapDocument != null && _mapDocument.Application is IMapApplication)
                ((IMapApplication)_mapDocument.Application).ValidateUI();
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("9617B354-6757-4c82-8B0F-F7987D7587D5")]
    public class Zoom2Position : ITool,IToolButtonState
    {
        private IMapDocument _mapDocument = null;
        private GPSPoint _gpspoint = null;
        private DateTime _timeStamp = DateTime.Now;

        #region ITool Members

        public string Name
        {
            get { return "Zoom2Position"; }
        }

        public bool Enabled
        {
            get
            {
                if (Device.GPS == null) _gpspoint = null;
                return (_mapDocument != null && Device.GPS != null);
            }
        }

        public string ToolTip
        {
            get { return "Zoom To GPS Position"; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get
            {
                return (new Buttons()).imageList1.Images[0];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _mapDocument = (IMapDocument)hook;
        }

        public void OnEvent(object MapEvent)
        {
            if (_mapDocument == null || Device.GPS == null || _mapDocument.Application == null) return;

            IMap map = _mapDocument.FocusMap;
            if (map == null || map.Display == null) return;
            GPSDevice gps = Device.GPS;
            if (gps == null) return;

            if (_gpspoint != null)  // Punkt aus der Karte entfernen
            {
                gps.PositionReceived -= new GPSDevice.PositionReceivedEventHandler(gps_PositionReceived);
                map.Display.GraphicsContainer.Elements.Remove(_gpspoint);
                _gpspoint = null;

                if (_mapDocument.Application is IMapApplication)
                    ((IMapApplication)_mapDocument.Application).RefreshActiveMap(DrawPhase.Graphics);
                return;
            }

            if (map.Display == null || map.Display.SpatialReference == null)
            {
                MessageBox.Show("No Spatialreference definded for current map...");
                return;
            }

            IPoint point = GeometricTransformer.Transform2D(new Point(gps.Longitude, gps.Latitude),
                SpatialReference.FromID("EPSG:4326"), map.Display.SpatialReference) as IPoint;

            if (point != null)
            {
                double width = map.Display.Envelope.maxx - map.Display.Envelope.minx;
                double height = map.Display.Envelope.maxy - map.Display.Envelope.miny;

                map.Display.ZoomTo(new Envelope(point.X - width / 2, point.Y - height / 2, point.X + width / 2, point.Y + height / 2));
                map.Display.GraphicsContainer.Elements.Add(_gpspoint=new GPSPoint(gps.Longitude, gps.Latitude, true));

                if(_mapDocument.Application is IMapApplication)
                    ((IMapApplication)_mapDocument.Application).RefreshActiveMap(DrawPhase.All);

                _timeStamp = DateTime.Now;
                gps.PositionReceived += new GPSDevice.PositionReceivedEventHandler(gps_PositionReceived);
            }
        }

        private void gps_PositionReceived(GPSDevice sender, string latitude, string longitude, double deciLat, double deciLon)
        {
            if (((TimeSpan)(DateTime.Now - _timeStamp)).TotalSeconds < 5) return;
            _timeStamp = DateTime.Now;

            if (_mapDocument == null || _mapDocument.FocusMap==null || Device.GPS == null || _mapDocument.Application == null || _gpspoint == null) return;

            if (!_mapDocument.FocusMap.Display.GraphicsContainer.Elements.Contains(_gpspoint)) return;

            IEnvelope env = _mapDocument.FocusMap.Display.Envelope;
            IPoint p = _gpspoint.Project(_mapDocument.FocusMap.Display);

            DrawPhase phase = DrawPhase.Graphics;

            if (p.X <= env.minx || p.X >= env.maxx ||
                p.Y <= env.miny || p.Y >= env.maxy)
            {
                double width = env.maxx - env.minx;
                double height = env.maxy - env.miny;

                _mapDocument.FocusMap.Display.ZoomTo(new Envelope(p.X - width / 2, p.Y - height / 2, p.X + width / 2, p.Y + height / 2));
                phase = DrawPhase.All;
            }

            if (_mapDocument.Application is IMapApplication)
                ((IMapApplication)_mapDocument.Application).RefreshActiveMap(phase);
        }

        #endregion

        #region IToolButtonState Member

        public bool Checked
        {
            get { return (_gpspoint != null); }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("74BCAB24-38FB-4600-B37C-64A7AE3AD702")]
    public class GPSTracking : ITool, IToolButtonState
    {
        private IMapDocument _mapDocument = null;
        private GPSTrack _track = null;

        #region ITool Member

        public string Name
        {
            get { return "GPS Track"; }
        }

        public bool Enabled
        {
            get
            {
                return (_mapDocument != null && Device.GPS != null);
            }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.GPS.Properties.Resources.freehand; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _mapDocument = (IMapDocument)hook;
        }

        public void OnEvent(object MapEvent)
        {
            if (_mapDocument == null || Device.GPS == null || _mapDocument.Application == null) return;

            IMap map = _mapDocument.FocusMap;
            if (map == null || map.Display == null) return;
            GPSDevice gps = Device.GPS;
            if (gps == null) return;

            if (_track == null)
            {
                _track = new GPSTrack(_mapDocument);
                _mapDocument.FocusMap.Display.GraphicsContainer.Elements.Add(_track);
            }
            else if (_track.IsConnected)
            {
                _track.Disconnect();
            }
            else
            {
                _track.Connect();
            }
        }

        #endregion

        #region IToolButtonState Member

        public bool Checked
        {
            get
            {
                return (_track != null && _track.IsConnected);
            }
        }

        #endregion
    }
}
