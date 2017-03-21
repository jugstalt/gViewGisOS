using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system.UI;
using gView.Framework.Db.UI;
using gView.Framework.Globalisation;

namespace gView.Interoperability.Sde.UI
{
    [gView.Framework.system.RegisterPlugIn("703F8E42-31D5-4647-BA0F-77A57809DF9A")]
    public class SdeExplorerGroupObject : ExplorerParentObject, IExplorerGroupObject, IPlugInDependencies
    {
        private IExplorerIcon _icon = new SdeConnectionsIcon();

        public SdeExplorerGroupObject() : base(null, null) { }

        #region IExplorerGroupObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "ESRI Sde Connections"; }
        }

        public string FullName
        {
            get { return "SdeConnections"; }
        }

        public string Type
        {
            get { return "SDE Connections"; }
        }

        public new object Object
        {
            get { return null; }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();

            base.AddChildObject(new SdeNewConnectionObject(this));

            ConfigTextStream stream = new ConfigTextStream("sde_connections");
            string connStr, id;
            while ((connStr = stream.Read(out id)) != null)
            {
                base.AddChildObject(new SdeExplorerObject(this, id, connStr));
            }
            stream.Close();
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            if (FullName == this.FullName)
            {
                SdeExplorerGroupObject exObject = new SdeExplorerGroupObject();
                cache.Append(exObject);
                return exObject;
            }
            return null;
        }

        #endregion

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return SdeDataset.hasUnsolvedDependencies;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("CCCFDF65-5830-4D9C-AC1F-8025C2F9427F")]
    public class SdeNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new SdeNewConnectionIcon();

        public SdeNewConnectionObject()
            : base(null, null)
        {
        }

        public SdeNewConnectionObject(IExplorerObject parent)
            : base(parent, null)
        {
        }

        #region IExplorerSimpleObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return LocalizedResources.GetResString("String.NewConnection", "New Connection..."); }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "New Sde Connection"; }
        }

        public void Dispose()
        {

        }

        public new object Object
        {
            get { return null; }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }
        #endregion

        #region IExplorerObjectDoubleClick Members

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            FormConnectionString dlg = new FormConnectionString();
            dlg.ProviderID = "arcsde";
            dlg.UseProviderInConnectionString = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string connStr = dlg.ConnectionString;
                if (connStr.ToLower().StartsWith("sde:"))
                    connStr = connStr.Substring(4, connStr.Length - 4);

                ConfigTextStream stream = new ConfigTextStream("sde_connections", true, true);
                string id = ConfigTextStream.ExtractValue(connStr, "server").Replace(":", "_").Replace(@"\", "_");
                string instance = ConfigTextStream.ExtractValue(connStr, "instance").Replace(":", "_").Replace(@"\", "_");
                string database = ConfigTextStream.ExtractValue(connStr, "database").Replace(":", "_").Replace(@"\", "_");

                if (String.IsNullOrEmpty(id))
                    id = "ArcSDE Connection";
                if (!String.IsNullOrEmpty(instance))
                    id += "-" + instance;
                if (!String.IsNullOrEmpty(database))
                    id += "-" + database;

                stream.Write(connStr, ref id);
                stream.Close();

                e.NewExplorerObject = new SdeExplorerObject(this.ParentExplorerObject, id, dlg.ConnectionString);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            return null;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is SdeExplorerGroupObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    public class SdeExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject
    {
        private SdeIcon _icon = new SdeIcon();
        private string _server = "", _connectionString = "", _errMsg = "";
        SdeDataset _dataset = null;

        public SdeExplorerObject() : base(null,typeof(SdeDataset)) { }
        public SdeExplorerObject(IExplorerObject parent, string server, string connectionString)
            : base(parent, typeof(SdeDataset))
        {
            _server = server;
            _connectionString = connectionString;
        }

        internal string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                return _server;
            }
        }

        public string FullName
        {
            get
            {
                return @"SdeConnections\" + _server;
            }
        }

        public string Type
        {
            get { return "SDE Feature Database"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_dataset != null)
            {
                _dataset.Dispose();
                _dataset = null;
            }
        }
        public object Object { get { return null; } }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();

            if (_dataset == null)
            {
                _dataset = new SdeDataset();
                _dataset.ConnectionString = this.ConnectionString;
                if (!_dataset.Open())
                {
                    MessageBox.Show(_dataset.lastErrorMsg);
                    return;
                }
            }
            foreach (IDatasetElement element in _dataset.Elements)
            {
                base.AddChildObject(new SdeFeatureClassExplorerObject(this, element));
            }

            base.SortChildObjects(new ExplorerObjectCompareByName());
            //dataset.Dispose(); // Connection gleich wieder löschen...
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.IndexOf(@"\"); // FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string dsName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            SdeExplorerGroupObject groupObject = new SdeExplorerGroupObject();
            if (groupObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in groupObject.ChildObjects)
            {
                if (exObject.Name == dsName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            ConfigTextStream stream = new ConfigTextStream("sde_connections", true, true);
            stream.Remove(this.Name, _connectionString);
            stream.Close();
            if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            return true;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public bool RenameExplorerObject(string newName)
        {
            bool ret = false;
            ConfigTextStream stream = new ConfigTextStream("sde_connections", true, true);
            ret = stream.ReplaceHoleLine(ConfigTextStream.BuildLine(_server, _connectionString), ConfigTextStream.BuildLine(newName, _connectionString));
            stream.Close();

            if (ret == true)
            {
                _server = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            }
            return ret;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("C81DCEA8-9037-44da-94D3-C7F06BA9C557")]
    public class SdeFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject, IPlugInDependencies
    {
        private string _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private SdeExplorerObject _parent;

        public SdeFeatureClassExplorerObject() : base(null,typeof(IFeatureClass)) { }
        public SdeFeatureClassExplorerObject(SdeExplorerObject parent, IDatasetElement element)
            : base(parent,typeof(IFeatureClass))
        {
            if (element == null) return;

            _fcname = element.Title;
            _parent = parent;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        _icon = new SdePolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        _icon = new SdePointIcon();
                        _type = "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        _icon = new SdeLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                }

            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get { return _fcname; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null) return "";
                return _parent.FullName + @"\" + this.Name;
            }
        }
        public string Type
        {
            get { return _type; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }
        public void Dispose()
        {
            if (_fc != null)
            {
                _fc = null;
            }
        }
        public object Object
        {
            get
            {
                if (_fc != null) return _fc;
                return null;
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string dsName = FullName.Substring(0, lastIndex);
            string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            SdeExplorerObject sdeObject = new SdeExplorerObject();
            sdeObject = sdeObject.CreateInstanceByFullName(dsName, cache) as SdeExplorerObject;
            if (sdeObject == null || sdeObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in sdeObject.ChildObjects)
            {
                if (exObject.Name == fcName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return SdeDataset.hasUnsolvedDependencies;
        }

        #endregion
    }


    class SdeConnectionsIcon : IExplorerIcon
    {
        System.Drawing.Image _image = (new Icons()).imageList1.Images[0];
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("2282F06E-E6B6-47a0-8719-C8BF42E04B2D");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return _image;
            }
        }

        #endregion
    }

    class SdeNewConnectionIcon : IExplorerIcon
    {
        System.Drawing.Image _image = (new Icons()).imageList1.Images[1];
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("93B20AD1-680A-4423-8006-717EF701F630");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return _image;
            }
        }

        #endregion
    }

    class SdeIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("292B5ABA-49C0-4eb5-AF54-894EE4A80A84");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return (new Icons()).imageList1.Images[2];
            }
        }

        #endregion
    }

    public class SdePointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("770393A6-FBDB-4f03-8CD8-95D8F99D27B4"); }
        }

        public System.Drawing.Image Image
        {
            get { return (new Icons()).imageList2.Images[4]; }
        }

        #endregion
    }

    public class SdeLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("B4C96550-E764-4452-94AF-EED010E16DC5"); }
        }

        public System.Drawing.Image Image
        {
            get { return (new Icons()).imageList2.Images[3]; }
        }

        #endregion
    }

    public class SdePolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("D30A31DE-4F4D-4495-AFD9-F94922BFDAE4"); }
        }

        public System.Drawing.Image Image
        {
            get { return (new Icons()).imageList2.Images[5]; }
        }

        #endregion
    }
}
