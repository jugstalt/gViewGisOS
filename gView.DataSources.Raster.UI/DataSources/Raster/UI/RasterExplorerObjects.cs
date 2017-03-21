using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.DataSources.Raster.File;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.system.UI;

namespace gView.DataSources.Raster.UI
{
    /*
    public class RasterCatalogExplorerObject : IExplorerFileObject
    {
        private IExplorerIcon _icon = new RasterIcon();
        private string _filename = "";
        private ImageCatalogLayer _layer = null;

        public RasterCatalogExplorerObject() { }
        public RasterCatalogExplorerObject(string filename)
        {
            _filename = filename;
        }
        #region IExplorerFileObject Members

        public string Filter
        {
            get { return "*.cat.mdb"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(_filename);
                    return fi.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return "Access Raster Catalog"; }
        }

        public void Dispose()
        {
            if (_layer != null)
            {
                _layer.Dispose();
                _layer = null;
            }
        }

        public new object Object
        {
            get
            {
                if (_layer == null)
                {
                    try
                    {
                        RasterFile dataset = new RasterFile();
                        _layer = (ImageCatalogLayer)dataset.AddRasterFile(_filename);

                        if (!_layer.isValid)
                        {
                            _layer.Dispose();
                            _layer = null;
                        }
                    }
                    catch { _layer = null; }
                }
                return _layer;
            }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return CreateInstance(FullName);
        }

        public IExplorerFileObject CreateInstance(string filename)
        {
            try
            {
                if (!(new FileInfo(filename).Exists)) return null;
            }
            catch { return null; }
            return new RasterCatalogExplorerObject(filename);
        }
        #endregion
    }
    */

    [gView.Framework.system.RegisterPlugIn("FD8B1495-5D9D-4e97-B640-BFEAC2C4DD4B")]
    public class RasterPyramidExplorerObject : ExplorerObjectCls, IExplorerFileObject
    {
        private IExplorerIcon _icon = new RasterIcon();
        private string _filename = "";
        private PyramidFileClass _class = null;

        public RasterPyramidExplorerObject() : base(null, typeof(PyramidFileClass)) { }
        public RasterPyramidExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(PyramidFileClass))
        {
            _filename = filename;
        }

        #region IExplorerFileObject Members

        public string Filter
        {
            get { return "*.jpg.mdb|*.png.mdb|*.tif.mdb"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(_filename);
                    return fi.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return "Raster Pyramid File"; }
        }

        public void Dispose()
        {
            if (_class != null)
            {
                _class.EndPaint(null);
                _class = null;
            }
        }

        public new object Object
        {
            get
            {
                if (_class == null)
                {
                    try
                    {
                        RasterFileDataset dataset = new RasterFileDataset();
                        IRasterLayer layer = (IRasterLayer)dataset.AddRasterFile(_filename);

                        if (layer != null && layer.Class is PyramidFileClass)
                        {
                            _class = (PyramidFileClass)layer.Class;
                            if (!_class.isValid)
                            {
                                _class.EndPaint(null);
                                _class = null;
                            }
                        }
                    }
                    catch { return _class; }
                }
                return _class;
            }
        }

        public IExplorerObject CreateInstanceByFullName(IExplorerObject parent, string FullName)
        {
            return CreateInstance(parent, FullName);
        }

        public IExplorerFileObject CreateInstance(IExplorerObject parent, string filename)
        {
            try
            {
                if (!(new FileInfo(filename)).Exists) return null;
            }
            catch { return null; }
            return new RasterPyramidExplorerObject(parent, filename);
        }
        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            try
            {
                FileInfo fi = new FileInfo(FullName);
                if (!fi.Exists) return null;

                RasterPyramidExplorerObject rObject = new RasterPyramidExplorerObject(null, FullName);
                if (rObject.Object is PyramidFileClass)
                {
                    cache.Append(rObject);
                    return rObject;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("6F0051F0-F3C7-4eee-BE4B-45340F684FAA")]
    public class RasterFileExplorerObject : ExplorerObjectCls, IExplorerFileObject
    {
        private IExplorerIcon _icon = new RasterIcon();
        private string _filename = "";
        private IRasterClass _class = null;

        public RasterFileExplorerObject() : base(null, typeof(IRasterClass)) { }
        private RasterFileExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(IRasterClass))
        {
            _filename = filename;
        }
        #region IExplorerFileObject Members

        public string Filter
        {
            get
            {
                //return "*.jpg|*.png|*.tif|*.tiff|*.pyc|*.sid|*.jp2";
                return "*.jpg|*.png|*.pyc|*.sid|*.jp2";
            }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(_filename);
                    return fi.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return "Raster File"; }
        }

        public void Dispose()
        {
            if (_class != null)
            {
                _class.EndPaint(null);
                _class = null;
            }
        }

        public new object Object
        {
            get
            {
                if (_class == null)
                {
                    try
                    {
                        RasterFileDataset dataset = new RasterFileDataset();
                        IRasterLayer layer = (IRasterLayer)dataset.AddRasterFile(_filename);

                        if (layer != null && layer.Class is IRasterClass)
                        {
                            _class = layer.Class as IRasterClass;
                            if (_class is RasterFileClass)
                            {
                                if (!((RasterFileClass)_class).isValid)
                                {
                                    _class.EndPaint(null);
                                    _class = null;
                                }
                            }
                            if (_class is PyramidFile)
                            {
                                if (!((PyramidFile)_class).isValid)
                                {
                                    _class.EndPaint(null);
                                    _class = null;
                                }
                            }
                        }
                    }
                    catch { return _class; }
                }
                return _class;
            }
        }

        public IExplorerObject CreateInstanceByFullName(IExplorerObject parent, string FullName)
        {
            return CreateInstance(parent, FullName);
        }
        public IExplorerFileObject CreateInstance(IExplorerObject parent, string filename)
        {
            try
            {
                if (!(new FileInfo(filename)).Exists) return null;
            }
            catch { return null; }
            return new RasterFileExplorerObject(parent, filename);
        }
        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            try
            {
                FileInfo fi = new FileInfo(FullName);
                if (!fi.Exists) return null;

                RasterPyramidExplorerObject rObject = new RasterPyramidExplorerObject(null, FullName);
                if (rObject.Object is IRasterClass)
                {
                    cache.Append(rObject);
                    return rObject;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }

    internal class RasterIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("5D01CC9D-1424-46e3-AA22-7282969B7062"); }
        }

        public System.Drawing.Image Image
        {
            get { return (new Icons()).imageList1.Images[0]; }
        }

        #endregion
    }
}
