using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Xml;

namespace gView.MapServer.Tasker.InstanceConnector
{
    public class InstanceConnection
    {
        private string _url = String.Empty;

        public InstanceConnection(string url)
        {
            _url = url;
        }

        public string Send(string OnlineResource, string service, string request, string InterpreterGUID)
        {
            return Send(OnlineResource, service, request, InterpreterGUID, String.Empty, String.Empty);
        }
        public string Send(string OnlineResource, string service, string request, string InterpreterGUID, string user, string pwd, int timeout = 0)
        {
            try
            {
                //return _instance.ServiceRequest2(OnlineResource, service, request, InterpreterGUID, user, HashPassword(pwd));

                // .NET Remoting
                using (MapServerInstanceTypeService conn = new MapServerInstanceTypeService(_url))
                {
                    if (timeout > 0) conn.Timeout = timeout * 1000;
                    return conn.ServiceRequest2(OnlineResource, service, request, InterpreterGUID, user, HashPassword(pwd));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                return "ERROR:" + ex.Message;
            }
        }

        public List<MapService> Services(string user, string password)
        {
            List<MapService> services = new List<MapService>();
            DateTime td = DateTime.Now;

            string axl = String.Empty;
            using (MapServerInstanceTypeService service = new MapServerInstanceTypeService(_url))
            {
                axl = service.ServiceRequest("catalog", "<GETCLIENTSERVICES/>", "BB294D9C-A184-4129-9555-398AA70284BC",
                    user,
                    password);
            }

            TimeSpan ts = DateTime.Now - td;
            int millisec = ts.Milliseconds;

            if (axl == "") return services;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(axl);
            foreach (XmlNode mapService in doc.SelectNodes("//SERVICE[@name]"))
            {
                MapService.MapServiceType type = MapService.MapServiceType.MXL;
                if (mapService.Attributes["servicetype"] != null)
                {
                    switch (mapService.Attributes["servicetype"].Value.ToLower())
                    {
                        case "mxl":
                            type = MapService.MapServiceType.MXL;
                            break;
                        case "svc":
                            type = MapService.MapServiceType.SVC;
                            break;
                        case "gdi":
                            type = MapService.MapServiceType.GDI;
                            break;
                    }
                }
                services.Add(new MapService(mapService.Attributes["name"].Value, type));
            }
            return services;
        }
        public string QueryMetadata(string serivce)
        {
            return QueryMetadata(serivce, String.Empty, String.Empty);
        }
        public string QueryMetadata(string service, string user, string password)
        {
            try
            {
                using(MapServerInstanceTypeService conn = new MapServerInstanceTypeService(_url))
                return conn.GetMetadata(service, user, HashPassword(password));
            }
            catch (Exception ex)
            {
                return "ERROR:" + ex.Message;
            }
        }

        public bool UploadMetadata(string service, string metadata)
        {
            return UploadMetadata(service, metadata, String.Empty, String.Empty);
        }
        public bool UploadMetadata(string service, string metadata, string user, string password)
        {
            try
            {
                using(MapServerInstanceTypeService conn = new MapServerInstanceTypeService(_url))
                return conn.SetMetadata(service, metadata, user, HashPassword(password));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddMap(string name, string mxl, string usr, string pwd)
        {
            using (MapServerInstanceTypeService conn = new MapServerInstanceTypeService(_url))
                return conn.AddMap(name, mxl, usr, HashPassword(pwd));
        }
        public bool RemoveMap(string name, string usr, string pwd)
        {
            using (MapServerInstanceTypeService conn = new MapServerInstanceTypeService(_url))
                return conn.RemoveMap(name, usr, HashPassword(pwd));
        }

        public string Ping()
        {
            using (MapServerInstanceTypeService conn = new MapServerInstanceTypeService(_url))
            {
                conn.Timeout = 5000;
                return conn.Ping();
            }
        }

        #region PasswordHash
        private string HashPassword(string password)
        {
            if (String.IsNullOrEmpty(password)) return String.Empty;

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] byteValue = UTF8Encoding.UTF8.GetBytes(password);
            byte[] byteHash = md5.ComputeHash(byteValue);
            md5.Clear();

            return Convert.ToBase64String(byteHash);
        }
        #endregion
        
        public class MapService
        {
            public enum MapServiceType { MXL = 0, SVC = 1, GDI = 2 }
            private string _name;
            private MapServiceType _type;

            internal MapService(string name, MapServiceType type)
            {
                _name = name;
                _type = type;
            }

            public string Name { get { return _name; } }
            public MapServiceType Type
            {
                get { return _type; }
            }
        }
    }
}
