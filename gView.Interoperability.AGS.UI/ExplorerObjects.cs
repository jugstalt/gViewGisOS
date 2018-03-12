using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system.UI;
using gView.Framework.IO;
using gView.Framework.Globalisation;
using gView.Interoperability.AGS.Proxy;
using gView.Interoperability.AGS.Helper;
using gView.Framework.Web;
using gView.Interoperability.AGS.Dataset;

namespace gView.Interoperability.AGS.UI
{
    [gView.Framework.system.RegisterPlugIn("E5EE9AD4-1E1C-45ef-96C8-E4D4F35007CB")]
    public class AGSExplorerObjects : ExplorerParentObject, IExplorerGroupObject
    {
        IExplorerIcon _icon = new AGSIcon();

        public AGSExplorerObjects() : base(null, null, 0) { }

        #region IExplorerObject Member

        public string Name
        {
            get { return "ESRI ArcGis Server Connections"; }
        }

        public string FullName
        {
            get { return "ESRI.ArcGisServer"; }
        }

        public string Type
        {
            get { return "ESRI.ArcGisServer Connections"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
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

        #region IExplorerParentObject Member

        public override void Refresh()
        {
            base.Refresh();
            base.AddChildObject(new AGSNewConnectionExplorerObject(this));

            ConfigTextStream stream = new ConfigTextStream("AGS_connections");
            string connStr, id;
            while ((connStr = stream.Read(out id)) != null)
            {
                base.AddChildObject(new AGSConnectionExplorerObject(this, id, connStr));
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
                AGSExplorerObjects exObject = new AGSExplorerObjects();
                cache.Append(exObject);
                return exObject;
            }
            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("8B1A260D-A19F-4526-8216-30901F5A3DE1")]
    public class AGSNewConnectionExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new AGSNewConnectionIcon();

        public AGSNewConnectionExplorerObject() : base(null, null, 0) { }
        public AGSNewConnectionExplorerObject(IExplorerObject parent) : base(parent, null, 0) { }

        #region IExplorerObject Member

        public string Name
        {
            get { return LocalizedResources.GetResString("String.NewConnection", "New ArcGis Server Connection..."); }
        }

        public string FullName
        {
            get { return String.Empty; }
        }

        public string Type
        {
            get { return String.Empty; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
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

        #region IExplorerObjectDoubleClick Member

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            FormNewConnection dlg = new FormNewConnection();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string connStr = dlg.ConnectionString;
                ConfigTextStream stream = new ConfigTextStream("AGS_connections", true, true);
                string id = ConfigTextStream.ExtractValue(connStr, "server");
                if (id.IndexOf(":") != -1)
                {
                    id = id.Replace(":", " (Port=") + ")";
                }
                stream.Write(connStr, ref id);
                stream.Close();

                e.NewExplorerObject = new AGSConnectionExplorerObject(this.ParentExplorerObject, id, dlg.ConnectionString);
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
            return (parentExObject is AGSExplorerObjects);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    internal class AGSConnectionExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable
    {
        private string _name = String.Empty, _connectionString = String.Empty;
        private IExplorerIcon _icon = new AGSConnectionIcon();

        public AGSConnectionExplorerObject() : base(null, null, 0) { }
        internal AGSConnectionExplorerObject(IExplorerObject parent, string name, string connectionString)
            : base(parent, null, 0)
        {
            _name = name;
            _connectionString = connectionString;
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return _name; }
        }

        public string FullName
        {
            get { return @"gView.AGS\" + _name; }
        }

        public string Type
        {
            get { return "gView.AGS Connection"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
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

        #region IExplorerParentObject Member

        public override void Refresh()
        {
            base.Refresh();

            try
            {
                string server = ConfigTextStream.ExtractValue(_connectionString, "server");

                //string usr = ConfigTextStream.ExtractValue(_connectionString, "user");
                //string pwd = ConfigTextStream.ExtractValue(_connectionString, "pwd");
                //if (usr != "" || pwd != "")
                //{
                //    connector.setAuthentification(usr, pwd);
                //}

                foreach (ServiceDescription service in ArcServerHelper.MapServerServices(server, ProxySettings.Proxy("http://" + server)))
                {
                    base.AddChildObject(
                        new AGSServiceExplorerObject(this, service.Name, "server=" + server + ";service=" + service.Url));
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            AGSExplorerObjects group = new AGSExplorerObjects();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (AGSExplorerObjects)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

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
            ConfigTextStream stream = new ConfigTextStream("AGS_connections", true, true);
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
            ConfigTextStream stream = new ConfigTextStream("AGS_connections", true, true);
            ret = stream.ReplaceHoleLine(ConfigTextStream.BuildLine(_name, _connectionString), ConfigTextStream.BuildLine(newName, _connectionString));
            stream.Close();

            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            }
            return ret;
        }

        #endregion
    }

    public class AGSServiceExplorerObject : ExplorerObjectCls, IExplorerSimpleObject
    {
        private IExplorerIcon _icon = new AGSServiceIcon();
        private string _name = "", _connectionString = "";
        private AGSClass _class = null;
        private AGSConnectionExplorerObject _parent = null;

        internal AGSServiceExplorerObject(AGSConnectionExplorerObject parent, string name, string connectionString)
            : base(parent,typeof(AGSClass), 1)
        {
            _name = name;
            _connectionString = connectionString;
            _parent = parent;
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return _name; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null) return "";
                return _parent.FullName + @"\" + _name;
            }
        }

        public string Type
        {
            get { return "gView.ArcGis Server Service"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {

        }

        public new object Object
        {
            get
            {
                if (_class == null)
                {
                    AGSDataset dataset = new AGSDataset(_connectionString, _name);
                    //dataset.Open(); // kein open, weil sonst ein GET_SERVICE_INFO durchgeführt wird...
                    if (dataset.Elements.Count == 0)
                    {
                        dataset.Dispose();
                        return null;
                    }

                    _class = dataset.Elements[0].Class as AGSClass;
                }
                return _class;
            }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string cnName = FullName.Substring(0, lastIndex);
            string svName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            AGSConnectionExplorerObject cnObject = new AGSConnectionExplorerObject();
            cnObject = cnObject.CreateInstanceByFullName(cnName, cache) as AGSConnectionExplorerObject;
            if (cnObject == null || cnObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in cnObject.ChildObjects)
            {
                if (exObject.Name == svName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion
    }
}
