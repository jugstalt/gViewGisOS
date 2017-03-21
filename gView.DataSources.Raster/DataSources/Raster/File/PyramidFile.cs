using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.system;

namespace gView.DataSources.Raster.File
{
    internal struct PyramidFileHeader
    {
        public PyramidFileHeader(bool init)
        {
            id1 = (byte)'P';
            id2 = (byte)'Y';
            id3 = (byte)'R';
            id4 = (byte)'2';
            iWidth = iHeight = Levels = 0;
            X = Y = 0.0;
            dx1 = dx2 = dy1 = dy2 = 0.0;
            cellX = cellY = 0.0;
            Format = new Guid();
        }
        public byte id1;
        public byte id2;
        public byte id3;
        public byte id4;

        public int iWidth;
        public int iHeight;
        public Guid Format;
        public int Levels;
        public double X;
        public double Y;
        public double dx1,dx2,dy1,dy2;
        public double cellX,cellY;

        public unsafe byte[] ToBytes
        {
            get
            {
                byte[] arr = new byte[sizeof(PyramidFileHeader)];
                fixed (byte* parr = arr)
                {
                    *((PyramidFileHeader*)parr) = this;
                }
            
                return arr;
            }
        }

        public static unsafe PyramidFileHeader FromBytes(byte[] arr)
        {
            if (arr.Length < sizeof(PyramidFileHeader))
                throw new ArgumentException();

            PyramidFileHeader s;
            fixed (byte* parr = arr)
            {
                s = *((PyramidFileHeader*)parr);
            }
            return s;
        }

        public Polygon CreatePolygon()
        {
            Polygon polygon = new Polygon();
            Ring ring = new Ring();
            gView.Framework.Geometry.Point p1 = new gView.Framework.Geometry.Point(
                X - dx1 / 2.0 - dy1 / 2.0,
                Y - dx2 / 2.0 - dy2 / 2.0);

            ring.AddPoint(p1);
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dx1 * iWidth, p1.Y + dx2 * iWidth));
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dx1 * iWidth + dy1 * iHeight, p1.Y + dx2 * iWidth + dy2 * iHeight));
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dy1 * iHeight, p1.Y + dy2 * iHeight));
            polygon.AddRing(ring);

            return polygon;
        }

        public void Save(System.IO.Stream stream)
        {
            byte[] bytes = this.ToBytes;

            stream.Write(bytes, 0, bytes.Length);
        }
        public static unsafe bool Load(System.IO.Stream stream, out PyramidFileHeader header)
        {
            try
            {
                byte[] buffer = new byte[sizeof(PyramidFileHeader)];
                if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    header = new PyramidFileHeader();
                    return false;
                }
                header = PyramidFileHeader.FromBytes(buffer);
                return true;
            }
            catch
            {
                header = new PyramidFileHeader();
                return false;
            }
        }
    }

    internal struct PyramidLevelHeader
    {
        public int level,numPictures;
        public double cellX,cellY;

        public unsafe byte[] ToBytes
        {
            get
            {
                byte[] arr = new byte[sizeof(PyramidLevelHeader)];
                fixed (byte* parr = arr)
                {
                    *((PyramidLevelHeader*)parr) = this;
                }

                return arr;
            }
        }

        public static unsafe PyramidLevelHeader FromBytes(byte[] arr)
        {
            if (arr.Length < sizeof(PyramidLevelHeader))
                throw new ArgumentException();

            PyramidLevelHeader s;
            fixed (byte* parr = arr)
            {
                s = *((PyramidLevelHeader*)parr);
            }
            return s;
        }

        public void Save(System.IO.Stream stream)
        {
            byte[] bytes = this.ToBytes;

            stream.Write(bytes, 0, bytes.Length);
        }
        public static unsafe bool Load(System.IO.Stream stream, out PyramidLevelHeader header)
        {
            try
            {
                byte[] buffer = new byte[sizeof(PyramidLevelHeader)];
                if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    header = new PyramidLevelHeader();
                    return false;
                }
                header = PyramidLevelHeader.FromBytes(buffer);
                return true;
            }
            catch
            {
                header = new PyramidLevelHeader();
                return false;
            }
        }
    }

    internal struct PyramidPictureHeader
    {
        public int level;
        public long startPosition;
        public int streamLength;
        public int iWidth;
        public int iHeight;
        public double X;
        public double Y;
        public double dx1, dx2, dy1, dy2;
        public double cellX, cellY;
        
        public unsafe byte[] ToBytes
        {
            get
            {
                byte[] arr = new byte[sizeof(PyramidPictureHeader)];
                fixed (byte* parr = arr)
                {
                    *((PyramidPictureHeader*)parr) = this;
                }

                return arr;
            }
        }

        public static unsafe PyramidPictureHeader FromBytes(byte[] arr)
        {
            if (arr.Length < sizeof(PyramidPictureHeader))
                throw new ArgumentException();

            PyramidPictureHeader s;
            fixed (byte* parr = arr)
            {
                s = *((PyramidPictureHeader*)parr);
            }
            return s;
        }

        public Polygon CreatePolygon()
        {
            Polygon polygon = new Polygon();
            Ring ring = new Ring();
            gView.Framework.Geometry.Point p1 = new gView.Framework.Geometry.Point(
                X - dx1 / 2.0 - dy1 / 2.0,
                Y - dx2 / 2.0 - dy2 / 2.0);

            ring.AddPoint(p1);
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dx1 * iWidth, p1.Y + dx2 * iWidth));
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dx1 * iWidth + dy1 * iHeight, p1.Y + dx2 * iWidth + dy2 * iHeight));
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dy1 * iHeight, p1.Y + dy2 * iHeight));
            polygon.AddRing(ring);

            return polygon;
        }

        public void Save(System.IO.Stream stream)
        {
            byte[] bytes = this.ToBytes;

            stream.Write(bytes, 0, bytes.Length);
        }
        public static unsafe bool Load(System.IO.Stream stream, out PyramidPictureHeader header)
        {
            try
            {
                byte[] buffer = new byte[sizeof(PyramidPictureHeader)];
                if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    header = new PyramidPictureHeader();
                    return false;
                }
                header = PyramidPictureHeader.FromBytes(buffer);
                return true;
            }
            catch
            {
                header = new PyramidPictureHeader();
                return false;
            }
        }
    }

    public class PyramidFile : IRasterClass, gView.Framework.Data.IParentRasterLayer
    {
        private IDataset _dataset;
        private string _name,_filename;
        private ISpatialReference _sRef = null;
        private PyramidFileHeader _header;
        private List<PyramidLevelHeader> _levelHeader;
        private List<PyramidPictureHeader> _picHeader;
        private bool _isValid = false;
        private System.IO.Stream _stream = null;

        public PyramidFile(IDataset dataset, string filename)
        {
            _dataset = dataset;
            try
            {
                FileInfo fi = new FileInfo(filename);
                _filename = fi.FullName;
                _name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

                _isValid = ReadPYX(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".pyx");
            }
            catch
            {
                _name = "";
            }
        }

        public bool isValid
        {
            get { return _isValid; }
        }
        private bool ReadPYX(string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (!fi.Exists) return false;

                StreamReader reader = new StreamReader(fi.FullName);
                PyramidFileHeader.Load(reader.BaseStream, out _header);

                _levelHeader = new List<PyramidLevelHeader>();
                for (int i = 0; i < _header.Levels; i++)
                {
                    PyramidLevelHeader levHeader;
                    PyramidLevelHeader.Load(reader.BaseStream, out levHeader);
                    _levelHeader.Add(levHeader);
                }

                _picHeader = new List<PyramidPictureHeader>();
                foreach (PyramidLevelHeader levHeader in _levelHeader)
                {
                    for (int i = 0; i < levHeader.numPictures; i++)
                    {
                        PyramidPictureHeader picHeader;
                        PyramidPictureHeader.Load(reader.BaseStream, out picHeader);
                        _picHeader.Add(picHeader);
                    }
                }
                reader.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region IRasterClass Member

        public IPolygon Polygon
        {
            get { return _header.CreatePolygon(); } 
        }

        public System.Drawing.Bitmap Bitmap
        {
            get { return null; }
        }

        public double oX
        {
            get { return _header.X; }
        }

        public double oY
        {
            get { return _header.Y; }
        }

        public double dx1
        {
            get { return _header.dx1; }
        }

        public double dx2
        {
            get { return _header.dx2; }
        }

        public double dy1
        {
            get { return _header.dy1; }
        }

        public double dy2
        {
            get { return _header.dy2; }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        public void BeginPaint(gView.Framework.Carto.IDisplay display, ICancelTracker cancelTracker)
        {
            if (_stream != null) EndPaint(cancelTracker);
            _stream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void EndPaint(ICancelTracker cancelTracker)
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
        }

        public System.Drawing.Color GetPixel(double X, double Y)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IParentLayer Member

        private InterpolationMethod _interpolation = InterpolationMethod.Fast;
        public InterpolationMethod InterpolationMethod
        {
            get
            {
                return _interpolation;
            }
            set
            {
                _interpolation = value;
            }
        }

        public IRasterLayerCursor ChildLayers(gView.Framework.Carto.IDisplay display, string filterClause)
        {
            List<IRasterLayer> layers = new List<IRasterLayer>();

            double dpm = Math.Max(display.GraphicsContext.DpiX, display.GraphicsContext.DpiY) / 0.0254;
            double pix = display.mapScale / dpm;/*display.dpm;*/  // [m]

            // Level bestimmen
            int level = 1;
            foreach (PyramidLevelHeader levHeader in _levelHeader)
            {
                if (levHeader.cellX <= pix && levHeader.cellY <= pix)
                    level = levHeader.level;
            }

            IEnvelope dispEnvelope = display.Envelope;
            if (display.GeometricTransformer != null)
            {
                dispEnvelope = (IEnvelope)((IGeometry)display.GeometricTransformer.InvTransform2D(dispEnvelope)).Envelope;
            }
            IGeometryDef geomDef = new GeometryDef(geometryType.Polygon, null, true);

            foreach (PyramidPictureHeader picHeader in _picHeader)
            {
                if (picHeader.level != level) continue;

                IPolygon polygon = picHeader.CreatePolygon();
                if (gView.Framework.SpatialAlgorithms.Algorithm.IntersectBox(polygon, dispEnvelope))
                {
                    PyramidFileImageClass pClass = new PyramidFileImageClass(_stream, picHeader, polygon);
                    RasterLayer rLayer = new RasterLayer(pClass);
                    rLayer.InterpolationMethod = this.InterpolationMethod;
                    if (pClass.SpatialReference == null) pClass.SpatialReference = _sRef;
                    layers.Add(rLayer);
                }
            }
            return new SimpleRasterlayerCursor(layers);
        }

        #endregion
    }

    internal class PyramidFileImageClass : IRasterClass
    {
        private PyramidPictureHeader _header;
        private System.IO.Stream _stream;
        private IPolygon _polygon;
        private ISpatialReference _sRef = null;

        public PyramidFileImageClass(System.IO.Stream stream, PyramidPictureHeader header, IPolygon polygon)
        {
            _stream = stream;
            _header = header;
            _polygon = polygon;
        }

        #region IRasterClass Member

        public IPolygon Polygon
        {
            get { return _polygon; }
        }

        public System.Drawing.Bitmap Bitmap
        {
            get { return _bm; }
        }

        public double oX
        {
            get { return _header.X; }
        }

        public double oY
        {
            get { return _header.Y; }
        }

        public double dx1
        {
            get { return _header.dx1; }
        }

        public double dx2
        {
            get { return _header.dx2; }
        }

        public double dy1
        {
            get { return _header.dy1; }
        }

        public double dy2
        {
            get { return _header.dy2; }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        System.Drawing.Bitmap _bm = null;
        public void BeginPaint(gView.Framework.Carto.IDisplay display, ICancelTracker cancelTracker)
        {
            try
            {
                byte[] buffer = new byte[_header.streamLength];
                _stream.Position = _header.startPosition;
                _stream.Read(buffer, 0, _header.streamLength);
                _bm = (System.Drawing.Bitmap)ImageFast.FromStream(buffer);
            }
            catch
            {
                EndPaint(cancelTracker);
            }
        }

        public void EndPaint(ICancelTracker cancelTracker)
        {
            if (_bm != null)
            {
                _bm.Dispose();
                _bm = null;
            }
        }

        public System.Drawing.Color GetPixel(double X, double Y)
        {
            return System.Drawing.Color.Transparent;
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return "image"; }
        }

        public string Aliasname
        {
            get { return "image"; }
        }

        public IDataset Dataset
        {
            get { return null; }
        }

        #endregion
    }
}
