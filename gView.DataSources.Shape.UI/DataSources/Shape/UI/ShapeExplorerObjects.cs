using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system.UI;

namespace gView.DataSources.Shape.UI
{
    [gView.Framework.system.RegisterPlugIn("665E0CD5-B3DF-436c-91B4-D4C0B3ECA5B9")]
    public class ShapeFileExplorerObject : ExplorerObjectCls, IExplorerFileObject, ISerializableExplorerObject, IExplorerObjectRenamable, IExplorerObjectDeletable
    {
        private string _filename = "";
        private ShapeDataset _shapeDataset = null;
        private IDatasetElement _shape = null;
        private ShapeFileIcon _icon = null;

        public ShapeFileExplorerObject() : base(null, typeof(IFeatureClass)) { }
        public ShapeFileExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(IFeatureClass))
        {
            _filename = filename;
            OpenShape();
        }

        #region IExplorerFileObject Member

        public string Filter
        {
            get
            {
                return "*.shp";
            }
        }

        public IExplorerFileObject CreateInstance(IExplorerObject parent, string filename)
        {
            try
            {
                if (!(new FileInfo(filename)).Exists) return null;
            }
            catch { return null; }
            return new ShapeFileExplorerObject(parent, filename);
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get
            {
                //if (_shapeDataset == null)
                //    OpenShape();

                //if (_shape != null) return _shape.Title + ".shp";

                try
                {
                    FileInfo fi = new FileInfo(_filename);
                    return fi.Name;
                }
                catch { return "???"; }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return "ESRI Shapefile"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {
            if (_shapeDataset != null)
            {
                _shapeDataset.Dispose();
                _shapeDataset = null;
            }
        }

        public new object Object
        {
            get
            {
                if (_shapeDataset == null)
                    OpenShape();
                return ((_shape != null) ? _shape.Class : null);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            IExplorerObject obj = (cache.Contains(FullName)) ? cache[FullName] : CreateInstance(null, FullName);
            cache.Append(obj);
            return obj;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

        public bool RenameExplorerObject(string newName)
        {
            if (_shapeDataset == null)
                OpenShape();

            if (_shapeDataset != null)
            {
                FileInfo fi = new FileInfo(_filename);
                string name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

                int pos = newName.LastIndexOf(".");
                if (pos != -1)
                    newName = newName.Substring(0, pos);

                if (_shapeDataset.Rename(name, newName))
                {
                    _shape.Title = newName;

                    if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (_shapeDataset == null)
                OpenShape();

            if (_shapeDataset != null)
            {
                FileInfo fi = new FileInfo(_filename);
                string name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

                if (_shapeDataset.Delete(name))
                {
                    if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        #endregion

        private void OpenShape()
        {
            try
            {
                FileInfo fi = new FileInfo(_filename);

                _shapeDataset = new ShapeDataset();
                _shapeDataset.ConnectionString = fi.Directory.FullName;

                if (!_shapeDataset.Open() || (_shape = _shapeDataset[fi.Name]) == null)
                {
                    _shapeDataset.Dispose();
                    _shapeDataset = null;
                    _icon = new ShapeFileIcon(geometryType.Unknown);
                }
                else
                {
                    if (_shape.Class is IFeatureClass)
                        _icon = new ShapeFileIcon(((IFeatureClass)_shape.Class).GeometryType);
                    else
                        _icon = new ShapeFileIcon(geometryType.Unknown);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error");
            }
        }
    }

    internal class ShapeFileIcon : IExplorerIcon
    {
        private geometryType _geoType;
        public ShapeFileIcon(geometryType geoType)
        {
            _geoType = geoType;
        }
        #region IExplorerIcon Member

        public Guid GUID
        {
            get
            {
                switch (_geoType)
                {
                    case geometryType.Point:
                    case geometryType.Multipoint:
                        return new Guid("A6292F4F-2D10-4E6B-88B6-19E1A40430C3");
                    case geometryType.Polyline:
                        return new Guid("452D5383-E20D-4CA2-8B39-C376FCA291F1");
                    case geometryType.Polygon:
                    case geometryType.Envelope:
                        return new Guid("F29A4EA5-9015-453B-B330-BE16D6E2588F");
                    default:
                        return new Guid("BEF8780C-21D6-428D-8F90-2F0FEADCC48C");
                }
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                switch (_geoType)
                {
                    case geometryType.Point:
                    case geometryType.Multipoint:
                        return (new Buttons()).imageList1.Images[1];
                    case geometryType.Polyline:
                        return (new Buttons()).imageList1.Images[2];
                    case geometryType.Polygon:
                    case geometryType.Envelope:
                        return (new Buttons()).imageList1.Images[3];
                    default:
                        return (new Buttons()).imageList1.Images[0];
                }
            }
        }

        #endregion
    }
}
