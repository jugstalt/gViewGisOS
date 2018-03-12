using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system.UI;
using gView.Framework.UI;
using gView.Framework.Db.UI;
using gView.Framework.Db;
using gView.Framework.IO;
using System.Windows.Forms;
using gView.DataSources.EventTable;
using gView.Framework.Data;
using gView.Framework.Globalisation;

namespace gView.DataSources.EventTable.UI
{
    [gView.Framework.system.RegisterPlugIn("653DA8F8-D321-4701-861F-23317E9FEC4D")]
    public class EventTableGroupObject : ExplorerParentObject, IExplorerGroupObject
    {
        private EventTableConnectionsIcon _icon = new EventTableConnectionsIcon();

        public EventTableGroupObject()
            : base(null, null, 0)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return "Eventtable Connections"; }
        }

        public string FullName
        {
            get { return "EventTableConnections"; }
        }

        public string Type
        {
            get { return "Eventtable Connections"; }
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
                EventTableGroupObject exObject = new EventTableGroupObject();
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

            base.AddChildObject(new EventTableNewConnectionObject(this));

            ConfigConnections conStream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            Dictionary<string, string> DbConnectionStrings = conStream.Connections;
            foreach (string DbConnName in DbConnectionStrings.Keys)
            {
                EventTableConnection dbConn = new EventTableConnection();
                dbConn.FromXmlString(DbConnectionStrings[DbConnName]);
                base.AddChildObject(new EventTableObject(this, DbConnName, dbConn));
            }
        }
        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("F45B7E98-B20A-47bf-A45D-E78D52F36314")]
    public class EventTableNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new EventTableNewConnectionIcon();

        public EventTableNewConnectionObject()
            : base(null, null, 1)
        {
        }

        public EventTableNewConnectionObject(IExplorerObject parent)
            : base(parent, null, 1)
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
            get { return "New Connection..."; }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "New OracleSDO Connection"; }
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
            FormEventTableConnection dlg = new FormEventTableConnection();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ConfigConnections connStream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");

                EventTableConnection etconn = new EventTableConnection(
                    dlg.DbConnectionString,
                    dlg.TableName,
                    dlg.IdField, dlg.XField, dlg.YField,
                    dlg.SpatialReference);

                string id = connStream.GetName(dlg.TableName);
                connStream.Add(id, etconn.ToXmlString());

                e.NewExplorerObject = new EventTableObject(this.ParentExplorerObject, id, etconn);
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
            return (parentExObject is EventTableGroupObject);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("B498C801-D7F2-4e1d-AA09-7A3599549DD8")]
    public class EventTableObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectContextMenu, IExplorerObjectDeletable, IExplorerObjectRenamable
    {
        private EventTableConnection _etconn = null;
        private EventTableIcon _icon = new EventTableIcon();
        private ToolStripItem[] _contextItems = null;
        private string _name = String.Empty;
        private IFeatureClass _fc = null;

        public EventTableObject() : base(null, typeof(IFeatureClass), 1) { }
        public EventTableObject(IExplorerObject parent, string name, EventTableConnection etconn)
            : base(parent, typeof(IFeatureClass), 1)
        {
            _name = name;
            _etconn = etconn;

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
            item.Click += new EventHandler(ConnectionProperties_Click);
            items.Add(item);

            _contextItems = items.ToArray();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            if (_etconn == null) return;

            FormEventTableConnection dlg = new FormEventTableConnection();
            dlg.DbConnectionString = _etconn.DbConnectionString;
            dlg.TableName = _etconn.TableName;
            dlg.IdField = _etconn.IdFieldName;
            dlg.XField = _etconn.XFieldName;
            dlg.YField = _etconn.YFieldName;
            dlg.SpatialReference = _etconn.SpatialReference;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                EventTableConnection etcon = new EventTableConnection(
                    dlg.DbConnectionString,
                    dlg.TableName,
                    dlg.IdField, dlg.XField, dlg.YField,
                    dlg.SpatialReference);

                ConfigConnections connStream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");
                connStream.Add(dlg.TableName, etcon.ToXmlString());
                _etconn = etcon;
            }
        }

        #region IExplorerObject Member

        public string Name
        {
            get
            {
                if (_etconn == null)
                    return "???";
                return _name;
            }
        }

        public string FullName
        {
            get
            {
                if (_etconn == null)
                    return "???";
                return @"EventTableConnections\" + _name;
            }
        }

        public string Type
        {
            get { return "Database Event Table"; ; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public object Object
        {
            get
            {
                if (_fc != null) return _fc;

                if (_etconn != null)
                {
                    try
                    {
                        Dataset ds = new Dataset();
                        ds.ConnectionString = _etconn.ToXmlString();
                        ds.Open();
                        _fc = ds.Elements[0].Class as IFeatureClass;
                        return _fc;
                    }
                    catch
                    {
                        _fc = null;
                    }
                }
                return null;
            }
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

            EventTableGroupObject group = new EventTableGroupObject();
            if (FullName.StartsWith(group.FullName))
            {
                foreach (IExplorerObject exObject in group.ChildObjects)
                {
                    if (exObject.FullName == FullName)
                    {
                        cache.Append(exObject);
                        return exObject;
                    }
                }
            }

            return null;
        }

        #endregion

        #region IExplorerObjectContextMenu Member

        public System.Windows.Forms.ToolStripItem[] ContextMenuItems
        {
            get { return _contextItems; }
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            ConfigConnections stream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            bool ret = stream.Remove(_name);

            if (ret)
            {
                if (ExplorerObjectDeleted != null)
                    ExplorerObjectDeleted(this);
            }
            return ret;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

        public bool RenameExplorerObject(string newName)
        {
            ConfigConnections stream = new ConfigConnections("eventtable", "546B0513-D71D-4490-9E27-94CD5D72C64A");
            bool ret = stream.Rename(_name, newName);

            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            }
            return ret;
        }

        #endregion
    }

    class EventTableConnectionsIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("10B98A8C-6E8E-4e92-B1F6-D9C923AF3151");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.DataSources.EventTable.UI.Properties.Resources.cat6;
            }
        }

        #endregion
    }

    class EventTableNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("BA81F595-A40D-45d0-9731-A2DC5D6B67BC");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.DataSources.EventTable.UI.Properties.Resources.gps_point;
            }
        }

        #endregion
    }

    class EventTableIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("C49DE671-C109-4e36-86D0-B9F4A1A75CC9");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.DataSources.EventTable.UI.Properties.Resources.table_base;
            }
        }

        #endregion
    }
}
