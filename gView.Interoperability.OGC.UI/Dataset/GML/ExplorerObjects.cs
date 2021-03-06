using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using System.IO;
using gView.Interoperability.OGC.Dataset.GML;
using gView.Framework.Data;
using gView.Framework.system.UI;

namespace gView.Interoperability.OGC.UI.Dataset.GML
{
    [gView.Framework.system.RegisterPlugIn("7FE05D6F-F480-4ee5-B592-934826A1C17A")]
    public class GMLExplorerObject : ExplorerParentObject, IExplorerFileObject, IExplorerObjectDeletable
    {
        private string _filename = "";
        internal gView.Interoperability.OGC.Dataset.GML.Dataset _dataset = null;
        private GMLIcon _icon = new GMLIcon();

        public GMLExplorerObject() : base(null, typeof(OGC.Dataset.GML.Dataset), 2) { }
        public GMLExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(OGC.Dataset.GML.Dataset),2)
        {
            _filename = filename;

            _dataset = new gView.Interoperability.OGC.Dataset.GML.Dataset();
            _dataset.ConnectionString = filename;

            _dataset.Open();
        }
        internal GMLExplorerObject(IExplorerObject parent, GMLExplorerObject exObject)
            : base(parent, typeof(OGC.Dataset.GML.Dataset), 2)
        {
            _filename = exObject._filename;
            _dataset = exObject._dataset;
        }
        #region IExplorerFileObject Member

        public string Filter
        {
            get { return "*.gml|*.xml"; }
        }

        public IExplorerFileObject CreateInstance(IExplorerObject parent, string filename)
        {
            GMLExplorerObject exObject = new GMLExplorerObject(null, filename);
            if (exObject._dataset.State == DatasetState.opened)
                return new GMLExplorerObject(null, exObject);

            return null;
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(_filename);
                    return fi.Name;
                }
                catch
                {
                    return "???.GML";
                }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return "OGC GML File"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public new object Object
        {
            get { return _dataset; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            try
            {
                GMLExplorerObject exObject = new GMLExplorerObject();
                exObject = exObject.CreateInstance(null, FullName) as GMLExplorerObject;
                if (exObject != null)
                {
                    cache.Append(exObject);
                }
                return exObject;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region IExplorerParentObject Member

        public override void Refresh()
        {
            base.Refresh();
            if (_dataset == null) return;

            foreach (IDatasetElement element in _dataset.Elements)
            {
                base.AddChildObject(new GMLFeatureClassExplorerObject(this, element.Title));
            }
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (_dataset != null)
            {
                if (_dataset.Delete())
                {
                    if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                    return true;
                }
            }
            return false;
        }

        #endregion
    }

    public class GMLFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject
    {
        private string _fcName;
        private GMLExplorerObject _parent;
        private IFeatureClass _fc;
        private GMLFeatureClassIcon _icon = new GMLFeatureClassIcon();

        public GMLFeatureClassExplorerObject(GMLExplorerObject parent, string fcName)
            : base(parent, typeof(IFeatureClass), 1)
        {
            _parent = parent;
            _fcName = fcName;

            if (this.Dataset != null)
            {
                IDatasetElement element = this.Dataset[fcName];
                if (element != null) _fc = element.Class as IFeatureClass;
            }
        }

        private gView.Interoperability.OGC.Dataset.GML.Dataset Dataset
        {
            get
            {
                if (_parent == null || !(_parent.Object is gView.Interoperability.OGC.Dataset.GML.Dataset))
                    return null;

                return _parent.Object as gView.Interoperability.OGC.Dataset.GML.Dataset;
            }
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return _fcName; }
        }

        public string FullName
        {
            get { return _parent.FullName + @"\" + Name; }
        }

        public string Type
        {
            get { return "GML Featureclass"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public new object Object
        {
            get { return _fc; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return null;
        }

        #endregion
    }

    class GMLIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("920FAAC4-4181-4729-A64B-65B02DF2883B"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Interoperability.OGC.UI.Properties.Resources.gml; }
        }

        #endregion
    }

    class GMLFeatureClassIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("238B1043-F758-4c90-8539-9988D76AA5D0"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Interoperability.OGC.UI.Properties.Resources.gml_layer; }
        }

        #endregion
    }
}
