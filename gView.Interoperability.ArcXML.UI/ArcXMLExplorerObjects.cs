using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.system.UI;
using gView.Framework.Globalisation;

namespace gView.Interoperability.ArcXML.Dataset
{
    [gView.Framework.system.RegisterPlugIn("FEEFD2E9-D0DD-4850-BCD6-86D88B543DB3")]
    public class ArcIMSExplorerObjects : ExplorerParentObject, IExplorerGroupObject
    {
        IExplorerIcon _icon = new ArcIMSIcon();

        public ArcIMSExplorerObjects() : base(null, null, 0) { }

        #region IExplorerObject Member

        public string Name
        {
            get { return "ESRI ArcIMS Services"; }
        }

        public string FullName
        {
            get { return "ESRI.ArcIMS"; }
        }

        public string Type
        {
            get { return "ESRI.ArcIMS Connections"; }
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
            base.AddChildObject(new ArcIMSNewConnectionExplorerObject(this));

            ConfigTextStream stream = new ConfigTextStream("ArcIMS_connections");
            string connStr, id;
            while ((connStr = stream.Read(out id)) != null)
            {
                base.AddChildObject(new ArcIMSConnectionExplorerObject(this, id, connStr));
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
                ArcIMSExplorerObjects exObject = new ArcIMSExplorerObjects();
                cache.Append(exObject);
                return exObject;
            }
            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("1B6F2AF5-9146-498C-9C71-8C69E153FD35")]
    public class ArcIMSNewConnectionExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        IExplorerIcon _icon = new ArcIMSNewConnectionIcon();

        public ArcIMSNewConnectionExplorerObject()
            : base(null, null, 0)
        {
        }

        public ArcIMSNewConnectionExplorerObject(IExplorerObject parent)
            : base(parent, null, 0)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return LocalizedResources.GetResString("String.NewConnection", "New ArcIMS Connection..."); }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return ""; }
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
                ConfigTextStream stream = new ConfigTextStream("ArcIMS_connections", true, true);
                string id = ConfigTextStream.ExtractValue(connStr, "server");
                if (id.IndexOf(":") != -1)
                {
                    id = id.Replace(":", " (Port=") + ")";
                }
                stream.Write(connStr, ref id);
                stream.Close();

                e.NewExplorerObject = new ArcIMSConnectionExplorerObject(this.ParentExplorerObject, id, dlg.ConnectionString);
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
            return (parentExObject is ArcIMSExplorerObjects);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return e.NewExplorerObject;
        }

        #endregion
    }

    internal class ArcIMSConnectionExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable
    {
        private string _name = "", _connectionString = "";
        private IExplorerIcon _icon = new ArcIMSConnectionIcon();

        public ArcIMSConnectionExplorerObject() : base(null, null, 0) { }
        internal ArcIMSConnectionExplorerObject(IExplorerObject parent, string name, string connectionString)
            : base(parent,null, 0)
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
            get { return @"gView.ArcIMS\" + _name; }
        }

        public string Type
        {
            get { return "gView.ArcIMS Connection"; }
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
                dotNETConnector connector = new dotNETConnector();

                string server = ConfigTextStream.ExtractValue(_connectionString, "server");
                string usr = ConfigTextStream.ExtractValue(_connectionString, "user");
                string pwd = ConfigTextStream.ExtractValue(_connectionString, "pwd");

                if (usr != "" || pwd != "")
                {
                    connector.setAuthentification(usr, pwd);
                }

                string axl = connector.SendRequest("<?xml version=\"1.0\" encoding=\"UTF-8\"?><GETCLIENTSERVICES/>", server, "catalog");
                if (axl == "") return;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(axl);
                foreach (XmlNode mapService in doc.SelectNodes("//SERVICE[@name]"))
                {
                    base.AddChildObject(new ArcIMSServiceExplorerObject(this, mapService.Attributes["name"].Value, _connectionString));
                }
                if (doc.SelectNodes("//SERVICE[@name]").Count == 0)
                {
                    foreach (XmlNode mapService in doc.SelectNodes("//SERVICE[@NAME]"))
                    {
                        base.AddChildObject(new ArcIMSServiceExplorerObject(this, mapService.Attributes["NAME"].Value, _connectionString));
                    }
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

            ArcIMSExplorerObjects group = new ArcIMSExplorerObjects();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (ArcIMSExplorerObjects)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

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
            ConfigTextStream stream = new ConfigTextStream("ArcIMS_connections", true, true);
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
            ConfigTextStream stream = new ConfigTextStream("ArcIMS_connections", true, true);
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

    //[gView.Framework.system.RegisterPlugIn("B164A6F4-42C8-4fe9-827E-48642E28C53E")]
    public class ArcIMSServiceExplorerObject : ExplorerObjectCls, IExplorerSimpleObject
    {
        private IExplorerIcon _icon = new ArcIMSServiceIcon();
        private string _name = "", _connectionString = "";
        private ArcIMSClass _class = null;
        private ArcIMSConnectionExplorerObject _parent = null;

        internal ArcIMSServiceExplorerObject(ArcIMSConnectionExplorerObject parent, string name, string connectionString)
            : base(parent,typeof(ArcIMSClass), 1)
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
            get { return "gView.ArcIMS Service"; }
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
                    ArcIMSDataset dataset = new ArcIMSDataset(_connectionString, _name);
                    //dataset.Open(); // kein open, weil sonst ein GET_SERVICE_INFO durchgeführt wird...
                    if (dataset.Elements.Count == 0)
                    {
                        dataset.Dispose();
                        return null;
                    }

                    _class = dataset.Elements[0].Class as ArcIMSClass;
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

            ArcIMSConnectionExplorerObject cnObject = new ArcIMSConnectionExplorerObject();
            cnObject = cnObject.CreateInstanceByFullName(cnName, cache) as ArcIMSConnectionExplorerObject;
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

    internal class ArcIMSIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("AB440AE4-94FA-4670-9CFA-1E5EEAECEE48"); }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return gView.Interoperability.ArcXML.UI.Properties.Resources.computer;
            }
        }

        #endregion
    }
    internal class ArcIMSNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("60124361-8D3E-48a2-81DE-A67FFBB4652A"); }
        }

        public System.Drawing.Image Image
        {
            get { return (new Icons()).imageList1.Images[1]; }
        }

        #endregion
    }
    internal class ArcIMSConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("11D02247-A596-485d-813E-C9608A37A752"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Interoperability.ArcXML.UI.Properties.Resources.computer_go; }
        }

        #endregion
    }
    internal class ArcIMSServiceIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("62A82B18-7F38-45c3-8C46-3995DE2E1190"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Interoperability.ArcXML.UI.Properties.Resources.i_connection; }
        }

        #endregion
    }
}
