using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.Geometry;
using gView.MapServer;
using gView.Framework.IO;
using System.Runtime.Remoting.Messaging;
using System.Data;
using gView.Framework.MapServer;

namespace gView.MapServer.Instance
{
    public class MapServerInstanceType : MarshalByRefObject, IMapServerInstanceService
    {
        private ServerMapDocument _doc;
        private bool _log_requests, _log_request_details, _log_errors;
        private int _countRequests = 0;

        public MapServerInstanceType()
        {
            _doc = IMS.mapDocument;

            _log_requests = Functions.log_requests;
            _log_request_details = Functions.log_request_details;
            _log_errors = Functions.log_errors;
        }

        private bool NodeAttributeBool(XmlNode node, string attr)
        {
            try
            {
                if (node.Attributes[attr] == null) return false;
                return Convert.ToBoolean(node.Attributes[attr].Value);
            }
            catch
            {
                return false;
            }
        }
        private int NodeAttributeInt(XmlNode node, string attr)
        {
            try
            {
                if (node.Attributes[attr] == null) return 0;
                return Convert.ToInt32(node.Attributes[attr].Value);
            }
            catch
            {
                return 0;
            }
        }
        private double NodeAttributeDouble(XmlNode node, string attr)
        {
            try
            {
                if (node.Attributes[attr] == null) return 0.0;
                return Convert.ToDouble(node.Attributes[attr].Value.Replace(".", ","));
            }
            catch
            {
                return 0.0;
            }
        }

        public string ServiceRequest(string service, string request, string InterpreterGUID, string usr, string pwd)
        {
            return ServiceRequest2(String.Empty, service, request, InterpreterGUID, usr, pwd);
        }

        public string ServiceRequest2(string OnlineResource, string service, string request, string InterpreterGUID, string usr, string pwd)
        {
            //string clientAddress = CallContext.GetData("ClientAddress").ToString();
            try
            {
#if(DEBUG)
                Logger.LogDebug("Start SendRequest");
#endif
                Identity id = Identity.FromFormattedString(usr);
                id.HashedPassword = pwd;

                if (IMS.acl != null && !IMS.acl.HasAccess(id, pwd, service))
                    throw new UnauthorizedAccessException();   //return "ERROR: Not Authorized!";
                    
                if (service == "catalog" && IMS.acl != null)
                    IMS.acl.ReloadXmlDocument();

                if (service == "catalog" && request == "get_used_servers")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<servers>");
                    foreach (DataRow server in MapServer.GDIServers.Rows)
                    {
                        sb.Append("<server uri='" + (string)server["Server"] + "' port='" + (int)server["Port"] + "' />");
                    }
                    sb.Append("</servers>");
                    return sb.ToString();
                }
                if (service == "catalog" && request == "get_instance_info")
                {
                    StringBuilder sb = new StringBuilder();
                    MapServer mapServer = IMS.mapServer;
                    sb.Append("<instance_info>");
                    if (mapServer != null)
                    {
                        sb.Append("<output url='" + mapServer.OutputUrl + "' />");
                    }
                    sb.Append("</instance_info>");
                    return sb.ToString();
                }
                
                Guid guid = new Guid(InterpreterGUID);
                IServiceRequestInterpreter interperter = null;
                foreach (IServiceRequestInterpreter i in IMS._interpreters)
                {
                    if (PlugInManager.PlugInID(i) == guid)
                    {
                        interperter = i;
                        break;
                    }
                }
                if (interperter == null)
                {
                    return "FATAL ERROR: Unknown request interpreger...";
                }
                // User berechtigungen überprüfen


                ServiceRequest serviceRequest = new ServiceRequest(service, request);
                serviceRequest.OnlineResource = OnlineResource;
                //serviceRequest.UserName = usr;
                serviceRequest.Identity = id;

                IServiceRequestContext context = new ServiceRequestContext(
                    IMS.mapServer,
                    interperter,
                    serviceRequest);

                //ManualResetEvent resetEvent = new ManualResetEvent(false);
                //IMS.threadQueue.AddQueuedThread(interperter.Request, context, resetEvent);
                //resetEvent.WaitOne();
                IMS.threadQueue.AddQueuedThreadSync(interperter.Request, context);

                return serviceRequest.Response;

#if(DEBUG)
                Logger.LogDebug("SendRequest Finished");
#endif
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
                return "FATAL ERROR: " + ex.Message;
            }
        }

        public bool AddMap(string mapName, string MapXML, string usr, string pwd)
        {
            if (String.IsNullOrEmpty(MapXML))
            {
                return ReloadMap(mapName, usr, pwd);
            }

            if (IMS.acl != null && !IMS.acl.HasAccess(Identity.FromFormattedString(usr), pwd, "admin_addmap"))
                return false;

            if (IMS.mapServer.Maps.Count >= IMS.mapServer.MaxServices)
            {
                // Überprüfen, ob schon eine Service mit gleiche Namen gibt...
                // wenn ja, ist es nur einem Refresh eines bestehenden Services
                bool found = false;
                foreach (IMapService map in IMS.mapServer.Maps)
                {
                    if (map.Name == mapName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }

            if (MapXML.IndexOf("<IServiceableDataset") == 0)
            {
                XmlStream xmlStream = new XmlStream("");

                StringReader sr = new StringReader("<root>" + MapXML + "</root>");
                if (!xmlStream.ReadStream(sr)) return false;

                IServiceableDataset sds = xmlStream.Load("IServiceableDataset", null) as IServiceableDataset;
                if (sds != null && sds.Datasets != null)
                {
                    IMS.SaveServiceableDataset(sds, mapName);
                    AddMapService(mapName, MapServiceType.SVC);
                }
            }
            else if (MapXML.IndexOf("<ServiceCollection") == 0)
            {
                XmlStream stream = new XmlStream("");

                StringReader sr = new StringReader(MapXML);
                if (!stream.ReadStream(sr)) return false;

                IMS.SaveServiceCollection(mapName, stream);
            }
            else  // Map
            {
                XmlStream xmlStream = new XmlStream("IMap");

                StringReader sr = new StringReader(MapXML);
                if (!xmlStream.ReadStream(sr)) return false;

                Map map = new Map();
                map.Load(xmlStream);
                map.Name = mapName;

                foreach (IMap m in ListOperations<IMap>.Clone(_doc.Maps))
                {
                    if (m.Name == map.Name) _doc.RemoveMap(m);
                }

                if (!_doc.AddMap(map)) return false;
                AddMapService(mapName, MapServiceType.MXL);

                IMS.SaveConfig(map);
            }

            /*
            // Alle Layer sichtbar schalten...
            IEnum layers = map.MapLayers;
            IDatasetElement element;
            while((element=(IDatasetElement)layers.Next)!=null)
            {
                ITOCElement tocElement = map.TOC.GetTOCElement(element);
                if (tocElement != null) tocElement.LayerVisible = true;
            }
            */

            return true;
        }

        public bool RemoveMap(string mapName, string usr, string pwd)
        {
            if (IMS.acl != null && !IMS.acl.HasAccess(Identity.FromFormattedString(usr), pwd, "admin_removemap"))
                return false;

            bool found = false;

            foreach (IMapService service in ListOperations<IMapService>.Clone(IMS.mapServices))
            {
                if (service.Name == mapName)
                {
                    IMS.mapServices.Remove(service);
                    found = true;
                }
            }

            foreach (IMap m in ListOperations<IMap>.Clone(IMS.mapDocument.Maps))
            {
                if (m.Name == mapName)
                {
                    _doc.RemoveMap(m);
                    found = true;
                }
            }
            IMS.RemoveConfig(mapName);

            return found;
        }

        public string Ping()
        {
            return "gView MapServer Instance v" + gView.Framework.system.SystemVariables.gViewVersion.ToString();
        }

        public string GetMetadata(string mapName, string usr, string pwd)
        {
            if (IMS.acl != null && !IMS.acl.HasAccess(Identity.FromFormattedString(usr), pwd, "admin_metadata_get"))
                return "ERROR: Not Authorized!";

            if (!ReloadMap(mapName, usr, pwd)) return String.Empty;

            //if (IMS.mapServer == null || IMS.mapServer[mapName] == null)
            //    return String.Empty;

            FileInfo fi = new FileInfo(IMS.ServicesPath + @"\" + mapName + ".meta");
            if (!fi.Exists) return String.Empty;

            using (StreamReader sr = new StreamReader(fi.FullName))
            {
                string ret = sr.ReadToEnd();
                sr.Close();
                return ret;
            }
        }
        public bool SetMetadata(string mapName, string metadata, string usr, string pwd)
        {
            if (IMS.acl != null && !IMS.acl.HasAccess(Identity.FromFormattedString(usr), pwd, "admin_metadata_set"))
                return false;

            FileInfo fi = new FileInfo(IMS.ServicesPath + @"\" + mapName + ".meta");

            StringReader sr = new StringReader(metadata);
            XmlStream xmlStream = new XmlStream("");
            xmlStream.ReadStream(sr);
            xmlStream.WriteStream(fi.FullName);
            //StreamWriter sw = new StreamWriter(fi.FullName, false);
            //sw.Write(metadata);
            //sw.Close();

            return ReloadMap(mapName, usr, pwd);
        }

        public bool ReloadMap(string mapName, string usr, string pwd)
        {
            if (IMS.acl != null && !IMS.acl.HasAccess(Identity.FromFormattedString(usr), pwd, mapName))
                return false;

            if (_doc == null) return false;

            // Remove map(s) (GDI) from Document
            List<IMap> maps = new List<IMap>();
            foreach (IMap m in _doc.Maps)
            {
                if (mapName.Contains(","))   // wenn ',' -> nur GDI Service suchen
                {
                    if (mapName == m.Name)
                    {
                        maps.Add(m);
                        break;
                    }
                }
                else   // sonst alle Services (incl GDI) suchen und entfernen
                {
                    foreach (string name in m.Name.Split(','))
                    {
                        if (mapName == name)
                        {
                            maps.Add(m);
                            break;
                        }
                    }
                }
            }

            if (maps.Count == 0)
            {
                // Reload map...
                IServiceMap smap = IMS.mapServer[mapName];
            }
            else
            {
                foreach (IMap m in maps)
                {
                    _doc.RemoveMap(m);
                    // Reload map(s) (GDI)...
                    IServiceMap smap = IMS.mapServer[m.Name];
                }
            }

            return true;
        }

        private void AddMapService(string mapName, MapServiceType type)
        {
            foreach (IMapService service in IMS.mapServices)
            {
                if (service.Name == mapName)
                    return;
            }
            IMS.mapServices.Add(new MapService(mapName, type));
        }
    }
}
