using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.system.UI;
using gView.Framework.UI;
using gView.Framework.Globalisation;
using gView.Framework.IO;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.DataSources.TileCache.UI
{
    [gView.Framework.system.RegisterPlugIn("0d217509-0df9-4b8d-bd3b-b7390de5abde")]
    public class TileCacheGroupExplorerObject : ExplorerParentObject, IExplorerGroupObject
    {
        private IExplorerIcon _icon = new TileCacheGroupIcon();

        public TileCacheGroupExplorerObject()
            : base(null, null)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return "Tile Caches"; }
        }

        public string FullName
        {
            get { return "TileCache"; }
        }

        public string Type
        {
            get { return "Tile Cache Connections"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public new object Object
        {
            get { return null; }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            if (this.FullName == FullName)
            {
                TileCacheGroupExplorerObject exObject = new TileCacheGroupExplorerObject();
                cache.Append(exObject);
                return exObject;
            }

            return null;
        }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            base.AddChildObject(new TileCacheNewConnectionObject(this));
            base.AddChildObject(new TileCacheLocalCacheProperties(this));

            ConfigConnections conStream = new ConfigConnections("TileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");
            Dictionary<string, string> connStrings = conStream.Connections;
            foreach (string connString in connStrings.Keys)
            {
                base.AddChildObject(new TileCacheDatasetExplorerObject(this, connString, connStrings[connString]));
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("b279a036-56c9-499c-99a7-2ec490988be6")]
    public class TileCacheNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new TileCacheNewConnectionIcon();

        public TileCacheNewConnectionObject()
            : base(null, null) { }
        public TileCacheNewConnectionObject(IExplorerObject parent)
            : base(parent, null) { }

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
            get { return "New Tile Cache Connection"; }
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
            FormTileCacheConnection dlg = new FormTileCacheConnection();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ConfigConnections connStream = new ConfigConnections("TileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");

                string connectionString = dlg.ConnectionString;
                string id = dlg.TileCacheName;
                id = connStream.GetName(id);

                connStream.Add(id, connectionString);
                e.NewExplorerObject = new TileCacheDatasetExplorerObject(this.ParentExplorerObject, id, connectionString);
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
            return (parentExObject is TileCacheGroupExplorerObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("a4196c6a-0431-4f17-af9e-97b4a2abf8dd")]
    public class TileCacheLocalCacheProperties : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new TileCacheLocalCachePropertiesIcon();

        public TileCacheLocalCacheProperties()
            : base(null, null) { }
        public TileCacheLocalCacheProperties(IExplorerObject parent)
            : base(parent, null) { }

        #region IExplorerSimpleObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return LocalizedResources.GetResString("String.LocalCacheProperties", "Local Caching Properties..."); }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "Properties"; }
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
            FormLocalCacheProperties dlg = new FormLocalCacheProperties();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //ConfigConnections connStream = new ConfigConnections("TileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");

                //string connectionString = dlg.ConnectionString;
                //string id = dlg.TileCacheName;
                //id = connStream.GetName(id);

                //connStream.Add(id, connectionString);
                //e.NewExplorerObject = new TileCacheDatasetExplorerObject(this.ParentExplorerObject, id, connectionString);
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
            return (parentExObject is TileCacheGroupExplorerObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectDoubleClick(null);
            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("d2a50a10-205d-43fd-93f8-e1e517c6cab3")]
    public class TileCacheDatasetExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private IExplorerIcon _icon = new TileCacheDatasetIcon();
        private string _name = String.Empty, _connectionString = String.Empty;
        private ToolStripItem[] _contextItems = null;
        private Dataset _dataset = null;

        public TileCacheDatasetExplorerObject()
            : base(null, null) { }
        public TileCacheDatasetExplorerObject(IExplorerObject parent, string name, string connectionString)
            : base(parent, null)
        {
            _name = name;
            _connectionString = connectionString;

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
            item.Click += new EventHandler(ConnectionProperties_Click);
            items.Add(item);

            _contextItems = items.ToArray();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            FormTileCacheConnection dlg = new FormTileCacheConnection();
            dlg.ConnectionString = _connectionString;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ConfigConnections connStream = new ConfigConnections("TileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");
                connStream.Add(_name, this.ConnectionString = dlg.ConnectionString);
            }
        }

        internal string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
                _dataset = null;
            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string FullName
        {
            get
            {
                return @"TileCache\" + _name;
            }
        }

        public string Type
        {
            get { return "Tile Cache Dataset"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        new public void Dispose()
        {
            base.Dispose();
        }
        public object Object
        {
            get
            {
                if (_dataset == null)
                {
                    _dataset = new Dataset();
                    _dataset.ConnectionString = _connectionString;
                    if (_dataset.Open())
                        return _dataset;
                }
                return null;
            }
        }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();

            try
            {
                Dataset dataset = new Dataset();
                dataset.ConnectionString = _connectionString;
                dataset.Open();

                foreach (IDatasetElement element in dataset.Elements)
                {
                    if (element.Class is IRasterClass)
                        base.AddChildObject(new TileCacheLayerExplorerObject(this, element));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            TileCacheGroupExplorerObject group = new TileCacheGroupExplorerObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (TileCacheGroupExplorerObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

            foreach (IExplorerObject exObject in group.ChildObjects)
            {
                if (exObject.FullName == FullName)
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
            ConfigConnections stream = new ConfigConnections("TileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");
            stream.Remove(_name);

            if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            return true;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public bool RenameExplorerObject(string newName)
        {
            bool ret = false;
            ConfigConnections stream = new ConfigConnections("TileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");
            ret = stream.Rename(_name, newName);

            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            }
            return ret;
        }

        #endregion

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get { return _contextItems; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("89676c37-faad-4b18-b879-a170dbf146e7")]
    public class TileCacheLayerExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject
    {
        private string _fcname = "";
        private IExplorerIcon _icon =new TileCacheDatasetIcon();
        private IRasterClass _rc = null;
        private TileCacheDatasetExplorerObject _parent = null;

        public TileCacheLayerExplorerObject() : base(null, typeof(FeatureClass)) { }
        public TileCacheLayerExplorerObject(TileCacheDatasetExplorerObject parent, IDatasetElement element)
            : base(parent, typeof(FeatureClass))
        {
            if (element == null) return;

            _parent = parent;
            _fcname = element.Title;

            if (element.Class is IRasterClass)
            {
                _rc = (IRasterClass)element.Class;
                
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
                return _parent.FullName + @"\" + _fcname;
            }
        }
        public string Type
        {
            get { return "Tile Cache Raster"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }
        public void Dispose()
        {
            if (_rc != null)
            {
                _rc = null;
            }
        }
        public object Object
        {
            get
            {
                return _rc;
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

            string[] parts = FullName.Split('\\');
            if (parts.Length != 3) return null;
            TileCacheDatasetExplorerObject parent = new TileCacheDatasetExplorerObject();
            parent = parent.CreateInstanceByFullName(parts[0] + @"\" + parts[1], cache) as TileCacheDatasetExplorerObject;
            if (parent == null)
                return null;

            foreach (IExplorerObject exObject in parent.ChildObjects)
            {
                if (exObject.Name == parts[2])
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion
    }

    public class TileCacheGroupIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("9376e718-834c-403f-b9b5-3c818d7b6e5b"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Properties.Resources.tiles; }
        }

        #endregion
    }

    public class TileCacheNewConnectionIcon : IExplorerIcon
    {

        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("d2029ed4-209d-4a32-aac2-6a92c626064c"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Properties.Resources.pointer_new; }
        }

        #endregion
    }

    public class TileCacheDatasetIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("16837b6b-ebd8-4e5c-87c3-e792f4f9ca34"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Properties.Resources.tiles; }
        }

        #endregion
    }

    public class TileCacheLocalCachePropertiesIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("00e957bf-a0d3-452e-950d-b165489038a4"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Properties.Resources.properties; }
        }

        #endregion
    }
}
