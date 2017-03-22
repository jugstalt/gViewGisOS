using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using gView.Framework.system;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Collections.Specialized;
using gView.MapServer.Tasker.InstanceConnector;
using System.Xml;
using System.Net;
using System.Diagnostics;

namespace gView.MapServer.Tasker.Service
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true
        /*,UseSynchronizationContext = false*/)]
    public class TaskerServiceType : ITaskerService
    {
        private Dictionary<string, IServiceRequestInterpreter> _interpreters = new Dictionary<string, IServiceRequestInterpreter>();
        private MapServerConfig.ServerConfig _serverConfig = null;
        static private Random _random = new Random();

        public void Init(int port)
        {
            _serverConfig = MapServerConfig.ServerByPort(port);

            if (_interpreters.Keys.Count == 0)
            {
                List<IServiceRequestInterpreter> interpreters = new List<IServiceRequestInterpreter>();
                PlugInManager compMan = new PlugInManager();
                foreach (XmlNode interpreterNode in compMan.GetPluginNodes(Plugins.Type.IServiceRequestInterpreter))
                {
                    IServiceRequestInterpreter interpreter = compMan.CreateInstance(interpreterNode) as IServiceRequestInterpreter;
                    if (interpreter == null) continue;
                    interpreters.Add(interpreter);
                }
                foreach (IServiceRequestInterpreter interpreter in interpreters)
                {
                    string guid = PlugInManager.PlugInID(interpreter).ToString().ToLower();
                    string name = interpreter.IntentityName.ToLower();
                    if (_interpreters.ContainsKey(guid) || _interpreters.ContainsKey(name))
                        continue;

                    _interpreters.Add(guid, interpreter);
                    if (!String.IsNullOrEmpty(name))
                        _interpreters.Add(name, interpreter);
                }
            }
        }

        #region Html UI

        public Stream Index()
        {
            try
            {
                string html = LoadHtml("TaskerIndex.htm");

                #region Instances
                StringBuilder ul = new StringBuilder();
                ul.Append("<div class='listbox'><span style='font-weight:bold'>Worker Processes</span>");
                ul.Append("<ul>");
                for (int i = 0; i < _serverConfig.Instances.Length; i++)
                {
                    InstanceConnection conn = new InstanceConnection("localhost:" + _serverConfig.Instances[i].Port);
                    
                    ul.Append("<li class='servicelink'>Instance "+(i+1));
                    try
                    {
                        DateTime start = DateTime.Now;
                        string ping;
                        for (int p = 0; p < 20; p++)
                            ping = conn.Ping();
                        TimeSpan ts = DateTime.Now - start;

                        ul.Append(" ... running (" + conn.Ping() + ") ... " + Math.Round(ts.TotalMilliseconds / 20, 1) + "ms/ping");
                    }
                    catch (Exception ex)
                    {
                        ul.Append(" ... error (" + ex.Message + ")");
                    }
                    ul.Append("</li>");
                }
                ul.Append("</ul></div>");
                html = html.Replace("[INSTANCES]", ul.ToString());
                #endregion

                return HtmlStream(html);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        public Stream Catalog()
        {
            try
            {
                if (_serverConfig == null || _serverConfig.Instances.Length == 0)
                    return WriteException("ServiceType not initialized!");

                string user, pwd;
                var request = Request(out user, out pwd);
                NameValueCollection queryString = request.UriTemplateMatch.QueryParameters;

                InstanceConnection conn = new InstanceConnection("localhost:" + _serverConfig.Instances[0].Port);
                var services = conn.Services(user, pwd);

                if (queryString["format"] == "xml")
                {
                    #region Xml
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<RESPONSE><SERVICES>");
                    foreach (var service in services)
                    {
                        sb.Append("<SERVICE ");
                        sb.Append("NAME='" + service.Name + "' ");
                        sb.Append("name='" + service.Name + "' ");
                        sb.Append("type='" + service.Type.ToString() + "' ");
                        sb.Append("/>");
                    }
                    sb.Append("</SERVICES></RESPONSE>");

                    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                    context.ContentType = "text/xml; charset=UTF-8";
                    return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
                    #endregion
                }

                string html = LoadHtml("TaskerCatalog.htm");
                #region Services List
                StringBuilder ul = new StringBuilder();
                ul.Append("<div class='listbox'><span style='font-weight:bold'>Services</span>");
                ul.Append("<ul>");
                foreach (var service in services)
                {
                    ul.Append("<li class='servicelink'><a href='ServiceCapabilities?service=" + service.Name + "'>" + service.Name + "</a></li>");
                }
                ul.Append("</ul></div>");
                html = html.Replace("[SERVICES]", ul.ToString());
                #endregion
                return HtmlStream(html);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        public Stream RecycleAll()
        {
            try
            {
                string html = LoadHtml("TaskerRecycleAll.htm");

                #region Instances

                StringBuilder ul = new StringBuilder();
                ul.Append("<div class='listbox'><span style='font-weight:bold'>Worker Processes</span>");
                ul.Append("<ul>");

                for (int i = 0; i < _serverConfig.Instances.Length; i++)
                {
                    foreach (Process proc in Process.GetProcessesByName("gView.MapServer.Instance"))
                    {
                        try
                        {
                            if (GetProcessCommandLine(proc).Trim().EndsWith("-port " + _serverConfig.Instances[i].Port))
                            {
                                proc.Kill();

                                ul.Append("<li class='servicelink'>Instance " + (i + 1) + " port " + _serverConfig.Instances[i].Port);
                                ul.Append(" ... recycled");
                                ul.Append("</li>");
                            }
                        }
                        catch { }
                    }
                }

                ul.Append("</ul></div>");
                html = html.Replace("[INSTANCES]", ul.ToString());

                #endregion

                return HtmlStream(html);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        private string GetProcessCommandLine(Process process)
        {
            var commandLine = new StringBuilder(process.MainModule.FileName);

            commandLine.Append(" ");
            using (var searcher = new System.Management.ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                    commandLine.Append(" ");
                }
            }

            return commandLine.ToString();
        }

        public Stream ServiceCapabilities()
        {
            try
            {
                string user, pwd;
                var request = Request(out user, out pwd);
                NameValueCollection queryString = request.UriTemplateMatch.QueryParameters;

                string serviceName = queryString["service"];
                if (String.IsNullOrEmpty(serviceName))
                    return WriteException("mandatory parameter service missing!");

                string html = LoadHtml("TaskerServiceCapabilities.htm ");

                #region Capabilities
                StringBuilder sb = new StringBuilder();
                foreach (string key in _interpreters.Keys)
                {
                    Guid guid;
                    if (Guid.TryParse(key, out guid))
                        continue;

                    IServiceRequestInterpreter si = _interpreters[key];
                    if (String.IsNullOrEmpty(si.IntentityName))
                        continue;

                    string server = _serverConfig.OnlineResource;
                    string onlineResource = server + "/MapRequest/" + si.IntentityName + "/" + serviceName + "?";

                    sb.Append("<div class='listbox'><span style='font-weight:bold'>" + si.IntentityName.ToUpper() + "</span>");
                    InterpreterCapabilities caps = si.Capabilities;
                    if (caps != null && caps.Capabilities != null)
                    {
                        sb.Append("<table width=100%>");
                        sb.Append("<tr>");
                        sb.Append("<td><div class='itembox' width=100% style='font-weight:bold'>Version</div></td>");
                        sb.Append("<td><div class='itembox' width=100% style='font-weight:bold'>Request</div></td>");
                        sb.Append("<td><div class='itembox' width=100% style='font-weight:bold'>Method</div></td>");
                        sb.Append("<td><div class='itembox' width=100% style='font-weight:bold'>Example</div></td>");
                        sb.Append("<td></tr>");
                        foreach (InterpreterCapabilities.Capability cap in caps.Capabilities)
                        {
                            sb.Append("<tr>");
                            sb.Append("<td><div class='itembox' width=100%>" + cap.Version + "</div></td>");
                            sb.Append("<td><div class='itembox' width=100%>" + cap.Name + "</div></td>");
                            sb.Append("<td><div class='itembox' width=100%>" + cap.Method.ToString() + "</div></td>");
                            sb.Append("<td><div class='itembox' width=100%>");
                            if (cap is InterpreterCapabilities.LinkCapability)
                            {
                                string txt = ((InterpreterCapabilities.LinkCapability)cap).RequestLink;
                                txt = txt.Replace("{server}", server).Replace("{onlineresource}", onlineResource).Replace("{service}", serviceName);
                                sb.Append("<a href='" + txt + "'>" + txt + "</a>");
                            }
                            else if (cap is InterpreterCapabilities.SimpleCapability)
                            {
                                string txt = ((InterpreterCapabilities.SimpleCapability)cap).RequestText;
                                txt = txt.Replace("{server}", server).Replace("{onlineresource}", onlineResource).Replace("{service}", serviceName);
                                sb.Append(txt);
                            }
                            sb.Append("</div></td>");
                            sb.Append("<tr>");
                        }
                    }
                    sb.Append("</table>");
                    sb.Append("</div>");
                }
                html = html.Replace("[CAPABILITIES]", sb.ToString());
                #endregion

                html = html.Replace("[SERVICE]", serviceName);

                return HtmlStream(html);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        public Stream ExploreMap(string name)
        {
            string html = LoadHtml("TaskerMap.htm");

            return HtmlStream(html);
        }

        public Stream LoadHtmlFile(string name)
        {
            string txt = LoadHtml(name);
            return WriteValue(txt);
        }
        #endregion

        public Stream AddMap(string name, Stream data)
        {
            try
            {
                if (_serverConfig == null || _serverConfig.Instances.Length == 0)
                    return WriteException("ServiceType not initialized!");

                string input = (data != null ? new StreamReader(data).ReadToEnd() : String.Empty);

                MapServerConfig.ServerConfig.InstanceConfig config = _serverConfig.Instances[0];
                InstanceConnection conn = new InstanceConnection("localhost:" + config.Port);

                string user, pwd;
                var request = Request(out user, out pwd);

                object ret = conn.AddMap(name, input, user, pwd);

                for (int i = 1; i < _serverConfig.Instances.Length; i++)
                {
                    MapServerConfig.ServerConfig.InstanceConfig config2 = _serverConfig.Instances[i];
                    InstanceConnection conn2 = new InstanceConnection("localhost:" + config2.Port);
                    conn2.AddMap(name, String.Empty, user, pwd);  // Refrsh
                }

                return WriteValue(ret);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        public Stream RemoveMap(string name)
        {
            try
            {
                if (_serverConfig == null || _serverConfig.Instances.Length == 0)
                    return WriteException("ServiceType not initialized!");

                MapServerConfig.ServerConfig.InstanceConfig config = _serverConfig.Instances[0];
                InstanceConnection conn = new InstanceConnection("localhost:" + config.Port);

                string user, pwd;
                var request = Request(out user, out pwd);

                return WriteValue(
                    conn.RemoveMap(name, user, pwd)
                    );
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        public Stream GetMetadata(string name)
        {
            try
            {
                if (_serverConfig == null || _serverConfig.Instances.Length == 0)
                    return WriteException("ServiceType not initialized!");

                MapServerConfig.ServerConfig.InstanceConfig config = _serverConfig.Instances[0];
                InstanceConnection conn = new InstanceConnection("localhost:" + config.Port);

                string user, pwd;
                var request = Request(out user, out pwd);

                return WriteValue(
                    conn.QueryMetadata(name, user, pwd)
                    );
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        public Stream SetMetadata(string name, Stream data)
        {
            try
            {
                if (_serverConfig == null || _serverConfig.Instances.Length == 0)
                    return WriteException("ServiceType not initialized!");

                string input = (data != null ? new StreamReader(data).ReadToEnd() : String.Empty);

                MapServerConfig.ServerConfig.InstanceConfig config = _serverConfig.Instances[0];
                InstanceConnection conn = new InstanceConnection("localhost:" + config.Port);

                string user, pwd;
                var request = Request(out user, out pwd);

                return WriteValue(
                    conn.UploadMetadata(name, input, user, pwd)
                    );
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        public Stream MapRequestGet(string interpreter, string name)
        {
            return MapRequestPost(interpreter, name, null);
        }

        public Stream MapRequestPost(string interpreter, string name, Stream data)
        {
            try
            {
                if (_serverConfig == null || _serverConfig.Instances.Length == 0)
                    return WriteException("ServiceType not initialized!");

                if (IfMatch())
                {
                    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                    context.StatusCode = HttpStatusCode.NotModified;
                    return null;
                }

                //DateTime td = DateTime.Now;
                //Console.WriteLine("Start Map Request " + td.ToLongTimeString() + "." + td.Millisecond + " (" + name + ")");
                //System.Threading.Thread.Sleep(10000);

                string user, pwd;
                var request = Request(out user, out pwd);
                NameValueCollection queryString = KeysToLower(request.UriTemplateMatch.QueryParameters);
                string input = (data != null ? new StreamReader(data).ReadToEnd() : String.Empty);

                if (String.IsNullOrEmpty(input))
                {
                    StringBuilder parameters = new StringBuilder();
                    foreach (string key in queryString.Keys)
                    {
                        if (parameters.Length > 0) parameters.Append("&");
                        parameters.Append(key.ToUpper() + "=" + queryString[key]);
                    }
                    input = parameters.ToString();
                }

                int instanceNr = GetRandom(_serverConfig.Instances.Length/*  MapServerConfig.ServerCount*/);
                MapServerConfig.ServerConfig.InstanceConfig config = _serverConfig.Instances[instanceNr];
                InstanceConnection conn = new InstanceConnection("localhost:" + config.Port);

                string onlineResource = _serverConfig.OnlineResource + "/MapRequest/" + interpreter + "/" + name;

                IServiceRequestInterpreter requestInterpreter = GetInterpreter(interpreter);

                string ret = conn.Send(onlineResource, name, input, PlugInManager.PlugInID(requestInterpreter).ToString(), user, pwd,
                    input.Contains("tile:render") ? 3600 : 0);

                //Console.WriteLine("Finished map Request: " + (DateTime.Now - td).TotalMilliseconds + "ms (" + name + ")");
                if (ret.StartsWith("image:"))
                {
                    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                    context.ContentType = "image/png";

                    ret = ret.Substring(6, ret.Length - 6);
                    return WriteFile(ret);
                }
                if (ret.StartsWith("{"))
                {
                    try
                    {
                        var mapServerResponse = gView.Framework.system.MapServerResponse.FromString(ret);

                        OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                        context.ContentType = mapServerResponse.ContentType;

                        if (mapServerResponse.Expires != null)
                            AppendEtag((DateTime)mapServerResponse.Expires);

                        return WriteBytes(mapServerResponse.Data);
                    }
                    catch { }
                }

                return WriteValue(ret);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        public Stream TileWmts(string name, string cacheType, string origin, string epsg, string style, string level, string row, string col)
        {
            try
            {
                if (_serverConfig == null || _serverConfig.Instances.Length == 0)
                    return WriteException("ServiceType not initialized!");

                if (IfMatch())
                {
                    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                    context.StatusCode = HttpStatusCode.NotModified;
                    return null;
                }

                string user, pwd;
                var request = Request(out user, out pwd);

                int instanceNr = GetRandom(_serverConfig.Instances.Length/*  MapServerConfig.ServerCount*/);
                MapServerConfig.ServerConfig.InstanceConfig config = _serverConfig.Instances[instanceNr];
                InstanceConnection conn = new InstanceConnection("localhost:" + config.Port);

                IServiceRequestInterpreter requestInterpreter = GetInterpreter("wmts");

                string ret = conn.Send(String.Empty, name, cacheType + "/" + origin + "/" + epsg + "/" + style + "/~" + level + "/" + row + "/" + col, PlugInManager.PlugInID(requestInterpreter).ToString(), user, pwd);

                if (ret.StartsWith("image:"))
                {
                    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                    context.ContentType = "image/png";

                    ret = ret.Substring(6, ret.Length - 6);
                    return WriteFile(ret);
                }
                if (ret.StartsWith("{"))
                {
                    try
                    {
                        var mapServerResponse = gView.Framework.system.MapServerResponse.FromString(ret);

                        OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                        context.ContentType = mapServerResponse.ContentType;

                        if (mapServerResponse.Expires != null)
                            AppendEtag((DateTime)mapServerResponse.Expires);

                        return WriteBytes(mapServerResponse.Data);
                    }
                    catch { }
                }

                return WriteValue(ret);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        #region ArcXML
        public System.IO.Stream AxlGet()
        {
            return AxlPost(null);
        }

        public System.IO.Stream AxlPost(System.IO.Stream data)
        {
            try
            {
                if (_serverConfig == null || _serverConfig.Instances.Length == 0)
                    return WriteException("ServiceType not initialized!");

                string user, pwd;
                var request = Request(out user, out pwd);

                NameValueCollection queryString = KeysToLower(request.UriTemplateMatch.QueryParameters);

                //DateTime td = DateTime.Now;
                //Console.WriteLine("Start AXL Request " + td.ToLongTimeString() + "." + td.Millisecond + " (" + queryString["servicename"] + ")");

                string input = (data != null ? new StreamReader(data).ReadToEnd() : String.Empty);

                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;


                if (queryString["cmd"] != null)
                {
                    context.ContentType = "text/plain; charset=UTF-8";
                    switch (queryString["cmd"].ToLower())
                    {
                        case "ping":
                            //return new MemoryStream(Encoding.UTF8.GetBytes("IMS v4.0.1\n"));
                            return new MemoryStream(Encoding.UTF8.GetBytes("IMS v10.0.0\n"));
                        case "getversion":
                            //return new MemoryStream(Encoding.UTF8.GetBytes("Version:4.0.1\nBuild_Number:630.1700\n"));
                            return new MemoryStream(Encoding.UTF8.GetBytes("Version=10.0.0\nBuild_Number=183.2159\n"));
                    }
                }

                if (queryString["servicename"] == null)
                    return WriteException("Parameter SERVICENAME is requiered!");

                context.ContentType = "text/xml; charset=UTF-8";

                int instanceNr = GetRandom(_serverConfig.Instances.Length/*MapServerConfig.ServerCount*/);
                MapServerConfig.ServerConfig.InstanceConfig config = _serverConfig.Instances[instanceNr];
                InstanceConnection connection = new InstanceConnection("localhost:" + config.Port);

                string response = connection.Send(
                    Functions.OgcOnlineResource(_serverConfig),
                    queryString["servicename"], input, "BB294D9C-A184-4129-9555-398AA70284BC", user, pwd).Trim();

                //Console.WriteLine("Finished AXL Request: " + (DateTime.Now - td).TotalMilliseconds + "ms (" + queryString["servicename"] + ")");
                return new MemoryStream(Encoding.UTF8.GetBytes(response));
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }
        #endregion

        #region Helper

        private NameValueCollection KeysToLower(NameValueCollection nvc)
        {
            NameValueCollection ret = new NameValueCollection();
            if (nvc == null)
                return ret;

            foreach (string key in nvc.AllKeys)
            {
                if (key == null)
                    continue;
                ret.Add(key.ToLower(), nvc[key]);
            }
            return ret;
        }

        private Stream WriteException(string message)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "text/xml; charset=UTF-8";

            return new MemoryStream(Encoding.UTF8.GetBytes("<EXCEPTION>" + message + "</EXCEPTION>"));
        }

        private Stream WriteValue(object obj)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            if (obj is String && ((String)obj).StartsWith("<"))
            {
                string xml = (string)obj;
                if (xml.StartsWith("<html") || xml.StartsWith("<table") || xml.StartsWith("<div") ||
                    xml.StartsWith("<HTML") || xml.StartsWith("<TABLE") || xml.StartsWith("<DIV"))
                {
                    context.ContentType = "text/html; charset=UTF-8";
                }
                else
                {
                    context.ContentType = "text/xml; charset=UTF-8";
                }
            }
            else
            {
                context.ContentType = "text/plain; charset=UTF-8";
            }

            return new MemoryStream(Encoding.UTF8.GetBytes((obj == null ? "null" : obj.ToString())));
        }

        private Stream WriteFile(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    byte[] buffer = new byte[fi.Length];
                    fs.Read(buffer, 0, (int)fi.Length);
                    fs.Close();
                    return new MemoryStream(buffer);
                }
            }
            return null;
        }

        public Stream WriteBytes(byte[] data)
        {
            return new MemoryStream(data);
        }

        private Stream WriteUnauthorized()
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.StatusCode = System.Net.HttpStatusCode.Unauthorized;
            context.Headers[HttpResponseHeader.WwwAuthenticate] = "Basic"; // funktioniert leider nicht in Standalone WCF Services... Er erscheint keine Angabemaske!!
            return null;

            throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
        }

        private IncomingWebRequestContext Request(out string user, out string pwd)
        {
            var context = WebOperationContext.Current.IncomingRequest;

            user = pwd = String.Empty;
            string auth64 = context.Headers[HttpRequestHeader.Authorization];
            if (!String.IsNullOrEmpty(auth64) && auth64.StartsWith("Basic "))
            {
                auth64 = auth64.Substring(6, auth64.Length - 6);
                string auth = Encoding.ASCII.GetString(Convert.FromBase64String(auth64));
                int index = auth.IndexOf(":");
                if (index > 0)
                {
                    user = auth.Substring(0, index);
                    pwd = auth.Substring(index + 1, auth.Length - index - 1);
                }
            }

            return context;
        }

        private IServiceRequestInterpreter GetInterpreter(string name)
        {
            name = name.ToLower();
            if (_interpreters.ContainsKey(name))
                return _interpreters[name];

            return null;
        }

        private int GetRandom(int maxValue)
        {
            lock (_random)
                return _random.Next(maxValue);
        }

        #region Html

        private string LoadHtml(string filetitle)
        {
            StreamReader sr = new StreamReader(SystemVariables.ApplicationDirectory + @"\html\" + filetitle);
            string html = sr.ReadToEnd();
            sr.Close();

            sr = new StreamReader(SystemVariables.ApplicationDirectory + @"\html\TaskerStyles.css");
            string css = sr.ReadToEnd();
            sr.Close();

            html = html.Replace("[STYLES]", css);
            Version version = SystemVariables.gViewVersion;
            html = html.Replace("[VERSION]", version.Major + "." + version.Minor + " Build: " + version.Build + "." + version.Revision);

            return html;
        }

        private Stream HtmlStream(string html)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "text/html; charset=UTF-8";

            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }
        #endregion

        #region ETag

        private bool HasIfNonMatch(IncomingWebRequestContext context)
        {
            return context.Headers["If-None-Match"] != null;
        }

        private bool IfMatch()
        {
            try
            {
                IncomingWebRequestContext context = WebOperationContext.Current.IncomingRequest;

                if (HasIfNonMatch(context) == false)
                    return false;

                var etag = long.Parse(context.Headers["If-None-Match"]);

                DateTime etagTime = new DateTime(etag, DateTimeKind.Utc);
                if (DateTime.UtcNow > etagTime)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AppendEtag(DateTime expires)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

            context.Headers.Add("ETag", expires.Ticks.ToString());
            context.Headers.Add("Last-Modified", DateTime.UtcNow.ToString("R"));
            context.Headers.Add("Expires", expires.ToString("R"));
           
            //this.Response.CacheControl = "private";
            //this.Response.Cache.SetMaxAge(new TimeSpan(24, 0, 0));
        }

        #endregion

        #endregion
    }
}
