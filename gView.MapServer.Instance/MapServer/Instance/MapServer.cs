using System;
using System.Collections.Generic;
using System.Text;
using gView.MapServer;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.system;
using gView.Framework.Data;
using System.IO;
using gView.Framework.Geometry;
using gView.Framework.IO;
using System.Xml;
using System.Data;

namespace gView.MapServer.Instance
{
    class MapServer : IMapServer
    {
        public enum ServerLicType { Unknown = 0, Private = 1, Professional = 2, Express = 3 };
        private IMapDocument _doc;
        private bool _log_requests, _log_request_details, _log_errors;
        private object _lockThis = new object();
        private Dictionary<string, object> _lockers = new Dictionary<string, object>();
        //private int _maxGDIServers = int.MaxValue;
        private int _maxServices = int.MaxValue;
        static internal GDIServers GDIServers = new GDIServers();

        public MapServer(int port)
        {
            _doc = IMS.mapDocument;

            _log_requests = Functions.log_requests;
            _log_request_details = Functions.log_request_details;
            _log_errors = Functions.log_errors;
        }

        private IServiceMap Map(string name, IServiceRequestContext context)
        {
            try
            {
                if (_doc == null) return null;

                object locker = null;
                lock (_lockThis)
                {
                    if (!_lockers.ContainsKey(name))
                        _lockers.Add(name, new object());
                    locker = _lockers[name];
                }

                //lock (_lockThis)
                lock (locker)
                {
                    string alias = name;

                    IMapService ms = FindMapService(name);
                    if (ms is MapServiceAlias)
                    {
                        name = ((MapServiceAlias)ms).ServiceName;
                    }

                    return FindServiceMap(name, alias, context);
                }
            }
            catch(Exception ex)
            {
                Log("MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }

        #region IMapServer Member

        public List<IMapService> Maps
        {
            get
            {
                try
                {
                    return ListOperations<IMapService>.Clone(IMS.mapServices);
                }
                catch (Exception ex)
                {
                    Log("MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
                    return new List<IMapService>();
                }
            }
        }

        public IServiceMap this[string name]
        {
            get
            {
                return this.Map(name, null);
            }
        }
        public IServiceMap this[IMapService service]
        {
            get
            {
                if (service == null) return null;
                return this[service.Name];
            }
        }
        public IServiceMap this[IServiceRequestContext context]
        {
            get
            {
                try
                {
                    if (context == null || context.ServiceRequest == null) return null;
                    IServiceMap map = this.Map(context.ServiceRequest.Service, context);
                    if (map is ServiceMap)
                        ((ServiceMap)map).SetRequestContext(context);

                    return map;
                }
                catch (Exception ex)
                {
                    Log("MapServer.Map", loggingMethod.error, ex.Message + "\n" + ex.StackTrace);
                    return null;
                }
            }
        }

        public bool LoggingEnabled(loggingMethod method)
        {
            switch (method)
            {
                case loggingMethod.error:
                    return _log_errors;
                case loggingMethod.request:
                    return _log_requests;
                case loggingMethod.request_detail:
                case loggingMethod.request_detail_pro:
                    return _log_request_details;
            }

            return false;
        }
        public void Log(string header, loggingMethod method, string msg)
        {
            switch (method)
            {
                case loggingMethod.error:
                    if (_log_errors)
                    {
                        if (!String.IsNullOrEmpty(header))
                            msg = header + "\n" + msg;
                        Logger.Log(method, IMS.Port != 0 ? IMS.Port.ToString() : String.Empty, msg);
                    }
                    break;
                case loggingMethod.request:
                    if (_log_requests)
                    {
                        if (!String.IsNullOrEmpty(header))
                            msg = header + " - " + msg;
                        Logger.Log(method, IMS.Port != 0 ? IMS.Port.ToString() : String.Empty, msg);
                    }
                    break;
                case loggingMethod.request_detail:
                    if (_log_request_details)
                    {
                        if (!String.IsNullOrEmpty(header))
                            msg = header + "\n" + msg;
                        Logger.Log(method, IMS.Port != 0 ? IMS.Port.ToString() : String.Empty, msg);
                    }
                    break;
                case loggingMethod.request_detail_pro:
                    if (_log_request_details)
                    {
                        if (!String.IsNullOrEmpty(header))
                            header = header.Replace(".", "_");
                        Logger.Log(method,
                            (IMS.Port != 0 ? IMS.Port.ToString() : String.Empty) + "_" + header,
                            msg);
                    }
                    break;
            }
        }

        public string OutputUrl
        {
            get
            {
                return Functions.outputUrl;
            }
        }

        public string OutputPath
        {
            get
            {
                return Functions.outputPath;
            }
        }

        public string TileCachePath
        {
            get
            {
                return Functions.tileCachePath;
            }
        }

        public bool CheckAccess(IIdentity identity, string service)
        {
            if (IMS.acl == null) return true;
            return IMS.acl.HasAccess(identity, null, service);
        }

        #endregion

        private bool ServiceExists(string name)
        {
            foreach (IMapService service in IMS.mapServices)
            {
                if (service == null) continue;
                if (service.Name == name) return true;
            }
            return false;
        }

        private IServiceMap FindServiceMap(string name, string alias, IServiceRequestContext context)
        {
            Map map = FindMap(alias, context);
            if (map != null) return new ServiceMap(map, this);

            if (name.Contains(",") /* && _serverLicType == ServerLicType.gdi*/)
            {
                Map newMap = null;

                string[] names = name.Split(',');

                #region Alias Service auflösen...
                StringBuilder sb = new StringBuilder();
                foreach (string n in names)
                {
                    IMapService ms = FindMapService(n);
                    if (ms == null) return null;

                    if (sb.Length > 0) sb.Append(",");
                    if (ms is MapServiceAlias)
                    {
                        sb.Append(((MapServiceAlias)ms).ServiceName);
                    }
                    else
                    {
                        sb.Append(ms.Name);
                    }
                }
                names = sb.ToString().Split(',');
                #endregion
                Array.Reverse(names);

                //if (names.Length > _maxGDIServices)
                //{
                //    return null;
                //}

                foreach (string n in names)
                {
                    Map m1 = FindMap(n, context);
                    if (m1.Name == n)
                    {
                        if (newMap == null)
                        {
                            newMap = new Map(m1, true);
                        }
                        else
                        {
                            newMap.Append(m1, true);

                            // SpatialReference von am weitesten unten liegenden Karte übernehmen
                            // ist geschackssache...
                            if (m1.Display != null && m1.Display.SpatialReference != null)
                            {
                                newMap.Display.SpatialReference =
                                    m1.Display.SpatialReference.Clone() as ISpatialReference;
                            }
                        }
                    }
                }
                if (newMap != null)
                {
                    // alle webServiceThemes im TOC vereinigen...
                    if (newMap.TOC != null)
                    {
                        foreach (ITOCElement tocElement in newMap.TOC.Elements)
                        {
                            if (tocElement == null ||
                                tocElement.Layers == null) continue;

                            foreach (ILayer layer in tocElement.Layers)
                            {
                                if (layer is IWebServiceLayer)
                                {
                                    newMap.TOC.RenameElement(tocElement, newMap.Name + "_WebThemes");
                                    break;
                                }
                            }
                        }
                    }
                    newMap.Name = alias;
                    if (!IMS.mapDocument.AddMap(newMap))
                        return null;

                    bool found = false;
                    foreach (IMapService ms in IMS.mapServices)
                    {
                        if (ms != null &&
                            ms.Name == alias && ms.Type == MapServiceType.GDI)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found) IMS.mapServices.Add(new MapService(name, MapServiceType.GDI));

                    return new ServiceMap(newMap, this);
                }
            }

            return null;
        }

        private IMapService FindMapService(string name)
        {
            foreach (IMapService ms in IMS.mapServices)
            {
                if (ms == null) continue;
                if (ms.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) return ms;
            }
            return null;
        }

        private Map FindMap(string name, IServiceRequestContext context)
        {
            foreach (IMap map in IMS.mapDocument.Maps)
            {
                if (map.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && map is Map)
                    return (Map)map;
            }

            if (name.Contains(",")) return null;

            IMap m = IMS.LoadMap(name, context);
            if (m is Map)
                return (Map)m;

            return null;
        }

        //internal int MaxGDIServers
        //{
        //    get { return _maxGDIServers; }
        //}

        internal int MaxServices
        {
            get { return _maxServices; }
        }

        internal bool AppendGDIServers(IMap map)
        {
            if (map == null) return true;

            foreach (IDatasetElement element in map.MapElements)
            {
                if (element != null && element.Class is IWebServiceClass)
                {
                    IWebServiceClass wClass = (IWebServiceClass)element.Class;
                    if (wClass.Dataset == null || wClass.Dataset.ConnectionString == null) continue;

                    string connString = wClass.Dataset.ConnectionString.ToLower();
                    string server = ConfigTextStream.ExtractValue(connString, "server");
                    if (String.IsNullOrEmpty(server))
                        server = ConfigTextStream.ExtractValue(connString, "wms");
                    if (String.IsNullOrEmpty(server))
                        server = ConfigTextStream.ExtractValue(connString, "wfs");
                    if (String.IsNullOrEmpty(server))
                        server = ConfigTextStream.ExtractValue(connString, "url");
                    if (String.IsNullOrEmpty(server))
                        server = ConfigTextStream.ExtractValue(connString, "uri");
                    if (String.IsNullOrEmpty(server))
                        continue; // eigentlich ungültig... solte Exception werfen...

                    if (server.StartsWith("http://") || server.StartsWith("https://"))
                    {
                        Uri uri = new Uri(server);
                        server = uri.Host;
                    }

                    //if (!GDIServers.Contains(server) &&
                    //    GDIServers.Count >= _maxGDIServers) ret = false;

                    GDIServers.Add(server);
                }
            }

            //return (GDIServers.Count <= _maxGDIServers);
            return true;
        }
    }

    class MapService : IMapService
    {
        private string _filename = String.Empty, _name = String.Empty;
        private MapServiceType _type = MapServiceType.MXL;
        //private List<string> _servers = new List<string>();

        public MapService() { }
        public MapService(string filename, MapServiceType type)
        {
            _type = type;
            try
            {
                _filename = filename;
                FileInfo fi = new FileInfo(filename);
                _name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

                //GetServers();
            }
            catch { }
        }

        /*
        private void GetServers()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(_filename);

                if (_type == MapServiceType.SVC)
                {
                    XmlNode serviceableDsNode = doc.SelectSingleNode("//IServiceableDataset");
                    string server = String.Empty;
                    foreach (XmlNode child in serviceableDsNode.ChildNodes)
                    {
                        if (child == null || child.Attributes["value"] == null) continue;
                        string connstr = child.Attributes["value"].Value.ToLower();

                        server = MapService.ExtractServer(connstr);
                        if (!String.IsNullOrEmpty(server))
                            break;
                    }
                    _servers.Add(server);
                }
            }
            catch { }
        }

        static internal string ExtractServer(string connString)
        {
            string server = ConfigTextStream.ExtractValue(connString, "server");
            if (String.IsNullOrEmpty(server))
                server = ConfigTextStream.ExtractValue(connString, "wms");
            if (String.IsNullOrEmpty(server))
                server = ConfigTextStream.ExtractValue(connString, "wfs");
            if (String.IsNullOrEmpty(server))
                server = ConfigTextStream.ExtractValue(connString, "url");
            if (String.IsNullOrEmpty(server))
                server = ConfigTextStream.ExtractValue(connString, "uri");
            if (String.IsNullOrEmpty(server))
                return String.Empty;

            if (server.StartsWith("http://") || server.StartsWith("https://"))
            {
                Uri uri = new Uri(server);
                server = uri.Host;
            }

            return server;
        }
        

        public string[] GIDServers
        {
            get { return _servers.ToArray(); }
        }
         * */

        #region IMapService Member

        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        public MapServiceType Type
        {
            get { return _type; }
        }

        #endregion
    }

    class MapServiceAlias : MapService
    {
        string _serviceName;

        public MapServiceAlias(string alias, MapServiceType type, string serviceName)
            : base(alias, type)
        {
            _serviceName = serviceName;
        }

        public string ServiceName
        {
            get { return _serviceName; }
        }
    }

    class GDIServers
    {
        private static object LockThis = new object();
        private DataTable _table;

        public GDIServers()
        {
            _table = new DataTable();
            _table.Columns.Add("Port", typeof(int));
            _table.Columns.Add("Server", typeof(string));
            _table.Columns.Add("DateTime", typeof(DateTime));
        }

        public void Clear()
        {
            _table.Rows.Clear();
        }

        public bool Contains(string server)
        {
            return (_table.Select("Server='" + server + "'").Length > 0);
        }

        public int Count
        {
            get
            {
                return _table.Rows.Count;
            }
        }

        public void Add(string server)
        {
            lock (LockThis)
            {
                if (this.Contains(server)) return;

                DataRow row = _table.NewRow();
                row["Port"] = IMS.Port;
                row["Server"] = server;
                row["DateTime"] = DateTime.Now;

                _table.Rows.Add(row);
            }
        }

        public DataRow[] Rows
        {
            get
            {
                return _table.Select("", "DateTime");
            }
        }
    }
}
