using gView.DataSources.OracleGeometry;
using gView.Framework.Data;
using gView.Framework.Db;
using gView.Framework.Db.UI;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using gView.Framework.IO;
using gView.Framework.OGC.UI;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.Oracle.UI
{
    //[RegisterPlugIn("6F9DF597-9AE1-4A94-8BDC-A780C6DA0A6E")]
    public class OracleExplorerGroupObject : ExplorerParentObject, IOgcGroupExplorerObject, IPlugInDependencies
    {
        private IExplorerIcon _icon = new OracleIcon();

        public OracleExplorerGroupObject() : base(null, null) { }

        #region IExplorerGroupObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "Oracle"; }
        }

        public string FullName
        {
            get { return @"OGC\Oracle"; }
        }

        public string Type
        {
            get { return "Oracle Connection"; }
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
            base.AddChildObject(new OracleNewConnectionObject(this));

            ConfigConnections conStream = new ConfigConnections("oracle", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            Dictionary<string, string> DbConnectionStrings = conStream.Connections;
            foreach (string DbConnName in DbConnectionStrings.Keys)
            {
                DbConnectionString dbConn = new DbConnectionString();
                dbConn.FromString(DbConnectionStrings[DbConnName]);
                base.AddChildObject(new OracleExplorerObject(this, DbConnName, dbConn));
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            if (this.FullName == FullName)
            {
                OracleExplorerGroupObject exObject = new OracleExplorerGroupObject();
                cache.Append(exObject);
                return exObject;
            }

            return null;
        }

        #endregion

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return false;
            //return Dataset.hasUnsolvedDependencies;
        }

        #endregion
    }

    //[RegisterPlugIn("88C52DA3-6933-4529-BD41-A9D65A702E66")]
    public class OracleNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new OracleNewConnectionIcon();

        public OracleNewConnectionObject()
            : base(null, null)
        {
        }

        public OracleNewConnectionObject(IExplorerObject parent)
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
            get { return "New Oracle Connection"; }
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
            dlg.ProviderID = "oracle";
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DbConnectionString dbConnStr = dlg.DbConnectionString;
                ConfigConnections connStream = new ConfigConnections("oracle", "546B0513-D71D-4490-9E27-94CD5D72C64A");

                string connectionString = dbConnStr.ConnectionString;
                string id = ConfigTextStream.ExtractOracleValue(connectionString, "service_name") + "@" + ConfigTextStream.ExtractOracleValue(connectionString, "host");
                id = connStream.GetName(id);
                connStream.Add(id, dbConnStr.ToString());

                e.NewExplorerObject = new OracleExplorerObject(this.ParentExplorerObject, id, dbConnStr);
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
            return (parentExObject is OracleExplorerGroupObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    public class OracleExplorerObject : gView.Framework.OGC.UI.ExplorerObjectFeatureClassImport, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private OracleIcon _icon = new OracleIcon();
        private string _server = "";
        private IFeatureDataset _dataset;
        private DbConnectionString _connectionString;
        private ToolStripItem[] _contextItems = null;

        public OracleExplorerObject() : base(null, typeof(IFeatureDataset)) { }
        public OracleExplorerObject(IExplorerObject parent, string server, DbConnectionString connectionString)
            : base(parent, typeof(IFeatureDataset))
        {
            _server = server;
            _connectionString = connectionString;

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
            item.Click += new EventHandler(ConnectionProperties_Click);
            items.Add(item);

            _contextItems = items.ToArray();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            if (_connectionString == null) return;

            FormConnectionString dlg = new FormConnectionString(_connectionString);
            dlg.ProviderID = "oracle";
            dlg.UseProviderInConnectionString = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                DbConnectionString dbConnStr = dlg.DbConnectionString;

                ConfigConnections connStream = new ConfigConnections("oracle", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                connStream.Add(_server, dbConnStr.ToString());

                _connectionString = dbConnStr;
            }
        }

        internal string ConnectionString
        {
            get
            {
                return _connectionString.ConnectionString;
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
                return @"OGC\Oracle\" + _server;
            }
        }

        public string Type
        {
            get { return "Oracle Database"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        public object Object
        {
            get
            {
                if (_dataset != null) _dataset.Dispose();

                _dataset = new Dataset();
                _dataset.ConnectionString = _connectionString.ConnectionString;
                _dataset.Open();
                return _dataset;
            }
        }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            if (_connectionString == null) return;

            Dataset dataset = new Dataset();
            dataset.ConnectionString = _connectionString.ConnectionString;
            dataset.Open();

            List<IDatasetElement> elements = dataset.Elements;

            if (elements == null) return;
            foreach (IDatasetElement element in elements)
            {
                if (element.Class is IFeatureClass)
                {
                    base.AddChildObject(new OracleFeatureClassExplorerObject(this, element));
                }
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            OracleExplorerGroupObject group = new OracleExplorerGroupObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (OracleExplorerGroupObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

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
            bool ret = false;
            if (_connectionString != null)
            {
                ConfigConnections stream = new ConfigConnections("oracle", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                ret = stream.Remove(_server);
            }

            if (ret && ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            return ret;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public bool RenameExplorerObject(string newName)
        {
            bool ret = false;
            if (_connectionString != null)
            {
                ConfigConnections stream = new ConfigConnections("oracle", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                ret = stream.Rename(_server, newName);
            }
            if (ret == true)
            {
                _server = newName;
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

    //[gView.Framework.system.RegisterPlugIn("56E94E3B-CB00-4481-9293-AE45E2E360D2")]
    public class OracleFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject, IExplorerObjectDeletable, IPlugInDependencies
    {
        private string _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private OracleExplorerObject _parent = null;

        public OracleFeatureClassExplorerObject() : base(null, typeof(IFeatureClass)) { }
        public OracleFeatureClassExplorerObject(OracleExplorerObject parent, IDatasetElement element)
            : base(parent, typeof(IFeatureClass))
        {
            if (element == null || !(element.Class is IFeatureClass)) return;

            _parent = parent;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        _icon = new OraclePolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        _icon = new OraclePointIcon();
                        _type = "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        _icon = new OracleLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                }
            }
        }

        internal string ConnectionString
        {
            get
            {
                if (_parent == null) return "";
                return _parent.ConnectionString;
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
                return _fc;
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

            OracleExplorerObject dsObject = new OracleExplorerObject();
            dsObject = dsObject.CreateInstanceByFullName(dsName, cache) as OracleExplorerObject;
            if (dsObject == null || dsObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in dsObject.ChildObjects)
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

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (_parent.Object is IFeatureDatabase)
            {
                if (((IFeatureDatabase)_parent.Object).DeleteFeatureClass(this.Name))
                {
                    if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                    return true;
                }
                else
                {
                    MessageBox.Show("ERROR: " + ((IFeatureDatabase)_parent.Object).lastErrorMsg);
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return false;
            //return Dataset.hasUnsolvedDependencies;
        }

        #endregion
    }
}
