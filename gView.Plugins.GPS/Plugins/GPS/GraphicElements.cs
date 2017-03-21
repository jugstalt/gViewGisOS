using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.GPS;

namespace gView.Plugins.GPS
{
    internal class GPSPoint : IGraphicElement
    {
        private double _lon, _lat;
        private bool _hook;

        public GPSPoint(double lon, double lat, bool hook)
        {
            _lon = lon;
            _lat = lat;
            _hook = hook;
        }

        #region IGraphicElement Members

        internal IPoint Project(IDisplay display)
        {
            if (display.SpatialReference != null)
            {
                return GeometricTransformer.Transform2D(new Point(_lon, _lat),
                    SpatialReference.FromID("EPSG:4326"), display.SpatialReference) as IPoint;
            }
            else
            {
                return new Point(_lon, _lat); 
            }
        }

        public void Draw(IDisplay display)
        {
            if (_hook && Device.GPS != null)
            {
                _lon = Device.GPS.Longitude;
                _lat = Device.GPS.Latitude;
            }

            if (display == null || display.GraphicsContext == null) return;

            IPoint mapPoint = Project(display);

            if (mapPoint != null)
            {
                double x = mapPoint.X, y = mapPoint.Y;
                display.World2Image(ref x, ref y);

                using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red,2))
                {
                    display.GraphicsContext.DrawEllipse(pen, (float)x - 8, (float)y - 8, 16, 16);
                    display.GraphicsContext.DrawLine(pen, (float)x - 8, (float)y, (float)x + 8, (float)y);
                    display.GraphicsContext.DrawLine(pen, (float)x, (float)y - 8, (float)x, (float)y + 8);
                }
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("37D3FFDB-F847-4053-A08A-99895266D7EF")]
    public class GPSTrack : IGraphicElement, IPersistable
    {
        private Polyline _polyline;
        private ISymbol _symbol,_pointSymbol;
        private bool _connected;
        IMapDocument _doc;
        private IMap _map;

        public GPSTrack()
        {
            _polyline = new Polyline();
            _polyline.AddPath(new Path());

            _symbol = new SimpleLineSymbol();
            ((SimpleLineSymbol)_symbol).Color = System.Drawing.Color.Red;
            _pointSymbol = new SimplePointSymbol();
            ((SimplePointSymbol)_pointSymbol).Color = System.Drawing.Color.Red;
            ((SimplePointSymbol)_pointSymbol).Size = 5;
        }
        public GPSTrack(IMapDocument doc)
            : this()
        {
            _doc = doc;
            _map = doc.FocusMap;

            Connect();
        }

        public void Connect()
        {
            if (_connected) return;
            Device.GPS.PositionReceived += new GPSDevice.PositionReceivedEventHandler(GPS_PositionReceived);
            _connected = true;
        }
        public void Disconnect()
        {
            Device.GPS.PositionReceived -= new GPSDevice.PositionReceivedEventHandler(GPS_PositionReceived);
            _connected = false;
        }
        public bool IsConnected
        {
            get { return _connected; }
        }

        private IPolyline Project(IDisplay display)
        {
            if (display.SpatialReference != null)
            {
                return GeometricTransformer.Transform2D(_polyline,
                    SpatialReference.FromID("EPSG:4326"), display.SpatialReference) as IPolyline;
            }
            else
            {
                return _polyline;
            }
        }

        void GPS_PositionReceived(GPSDevice sender, string latitude, string longitude, double deciLat, double deciLon)
        {
            if (_polyline[0].PointCount > 0)
            {
                IPoint p = _polyline[0][_polyline[0].PointCount - 1];
                if (Math.Abs(p.X - deciLon) < 1e-10 &&
                    Math.Abs(p.Y - deciLat) < 1e-10) return;
            }
            
            _polyline[_polyline.PathCount-1].AddPoint(
                new TrackPoint(sender, deciLat, deciLon));

            //if (_doc != null && _doc.FocusMap != null && _doc.FocusMap == _map &&
            //    _doc.Application is IMapApplication)
            //{
            //    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
            //}
        }

        #region IGraphicElement Member

        public void Draw(IDisplay display)
        {
            IPolyline pLine = Project(display);
            if (pLine != null)
            {
                if (_pointSymbol != null)
                {
                    for (int i = 0; i < pLine[0].PointCount; i++)
                    {
                        display.Draw(_pointSymbol, pLine[0][i]);
                    }
                }
                if (_symbol != null)
                {
                    display.Draw(_symbol, pLine);
                }
            }
        }

        #endregion

        #region HelperClasses
        private class TrackPoint : IPoint, IPersistable
        {
            private double _lon, _lat, _h, _speed;
            private double _pdop, _hdop, _vdop;
            
            internal TrackPoint() {}
            public TrackPoint(GPSDevice sender, double deciLat, double deciLon)
            {
                _lat = deciLat;
                _lon = deciLon;
                _h = sender.EllipsoidHeight;
                _speed = sender.Speed;
                
                _pdop = sender.PDOP;
                _hdop = sender.HDOP;
                _vdop = sender.VDOP;
            }

            public double Longitude
            {
                get { return _lon; }
            }
            public double Latitude
            {
                get { return _lat; }
            }
            public double PDOP
            {
                get { return _pdop; }
            }
            public double VDOP
            {
                get { return _vdop; }
            }
            public double HDOP
            {
                get { return _hdop; }
            }

            #region IPoint Member

            public double X
            {
                get
                {
                    return _lon;
                }
                set
                {
                   
                }
            }

            public double Y
            {
                get
                {
                    return _lat;
                }
                set
                {
                    
                }
            }

            public double Z
            {
                get
                {
                    return _h;
                }
                set
                {
                    
                }
            }

            public double M
            {
                get { return 0.0; }
                set { }
            }
            #endregion

            #region IGeometry Member

            public geometryType GeometryType
            {
                get { return geometryType.Point; }
            }

            public IEnvelope Envelope
            {
                get { return new Envelope(this.X, this.Y, this.X, this.Y); }
            }

            public void Serialize(System.IO.BinaryWriter w, IGeometryDef geomDef)
            {
                
            }

            public void Deserialize(System.IO.BinaryReader r, IGeometryDef geomDef)
            {
                
            }

            public int? Srs { get; set; }

            #endregion

            #region ICloneable Member

            public object Clone()
            {
                return new Point(this.X, this.Y);
            }

            #endregion

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                _lon = (double)stream.Load("Lon", 0.0);
                _lat = (double)stream.Load("Lat", 0.0);
                _h = (double)stream.Load("h", 0.0);
                _speed = (double)stream.Load("Speed", 0.0);

                _pdop = (double)stream.Load("PDOP", 0.0);
                _vdop = (double)stream.Load("VDOP", 0.0);
                _hdop = (double)stream.Load("HDOP", 0.0);
            }

            public void Save(IPersistStream stream)
            {
                stream.Save("Lon", _lon);
                stream.Save("Lat", _lat);
                stream.Save("h", _h);
                stream.Save("Speed", _speed);

                stream.Save("PDOP", _pdop);
                stream.Save("VDOP", _vdop);
                stream.Save("HDOP", _hdop);
            }

            #endregion

            public override bool Equals(object obj)
            {
                return Equals(obj, 0.0);
            }
            public bool Equals(object obj, double epsi)
            {
                if (obj is IPoint)
                {
                    IPoint point = (IPoint)obj;
                    return Math.Abs(point.X - this.X) <= epsi &&
                           Math.Abs(point.Y - this.Y) <= epsi &&
                           Math.Abs(point.Z - this.Z) <= epsi;
                }
                return false;
            }

            #region IPoint Member


            public double Distance(IPoint p)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IPoint Member


            public double Distance2(IPoint p)
            {
                throw new NotImplementedException();
            }

            #endregion
        }
        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            TrackPoint point;
            while ((point = (TrackPoint)stream.Load("Point", null, new TrackPoint())) != null)
            {
                _polyline[0].AddPoint(point);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_polyline == null || _polyline.PathCount != 1) return;

            for (int i = 0; i < _polyline[0].PointCount; i++)
            {
                stream.Save("Point", _polyline[0][i]);
            }
        }

        #endregion
    }
}
