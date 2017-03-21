using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Xml;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.MapServer;
using gView.MapServer.Instance.Threading;
using System.Text;
using gView.Framework.Geometry;

namespace gView.MapServer.Instance
{
    class IMS
    {
        static public ServerMapDocument mapDocument = new ServerMapDocument();
        static public ThreadQueue<IServiceRequestContext> threadQueue = null;
        static internal string ServicesPath = String.Empty;
        static private int _port = 0;
        static internal string OutputPath = String.Empty;
        static internal string OutputUrl = String.Empty;
        static internal string TileCachePath = String.Empty;
        static internal List<IServiceRequestInterpreter> _interpreters = new List<IServiceRequestInterpreter>();
        static internal License myLicense = null;
        static internal List<IMapService> mapServices = new List<IMapService>();
        static internal MapServer mapServer = null;
        static internal Acl acl = null;

        internal static void LoadConfigAsync()
        {
            Thread thread = new Thread(new ThreadStart(LoadConfig));
            thread.Start();
        }
        internal static void LoadConfig()
        {
            try
            {
                if (mapDocument == null) return;

                DirectoryInfo di = new DirectoryInfo(ServicesPath);
                if (!di.Exists) di.Create();

                acl = new Acl(new FileInfo(ServicesPath + @"\acl.xml"));

                foreach (FileInfo fi in di.GetFiles("*.mxl"))
                {
                    try
                    {
                        if (mapServices.Count < mapServer.MaxServices)
                        {
                            MapService service = new MapService(fi.FullName, MapServiceType.MXL);
                            mapServices.Add(service);
                            Console.WriteLine("service " + service.Name + " added");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Functions.log_errors)
                            Logger.Log(loggingMethod.error, "LoadConfig - " + fi.Name + ": " + ex.Message);
                    }
                }
                foreach (FileInfo fi in di.GetFiles("*.svc"))
                {
                    try
                    {
                        if (mapServices.Count < mapServer.MaxServices)
                        {
                            MapService service = new MapService(fi.FullName, MapServiceType.SVC);
                            mapServices.Add(service);
                            Console.WriteLine("service " + service.Name + " added");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Functions.log_errors)
                            Logger.Log(loggingMethod.error, "LoadConfig - " + fi.Name + ": " + ex.Message);
                    }
                }

                try
                {
                    // Config Datei laden...
                    FileInfo fi = new FileInfo(ServicesPath + @"\config.xml");
                    if (fi.Exists)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(fi.FullName);

                        #region onstart - alias
                        foreach (XmlNode serviceNode in doc.SelectNodes("MapServer/onstart/alias/services/service[@alias]"))
                        {
                            string serviceName = serviceNode.InnerText;
                            MapService ms = new MapServiceAlias(
                                serviceNode.Attributes["alias"].Value,
                                serviceName.Contains(",") ? MapServiceType.GDI : MapServiceType.SVC,
                                serviceName);
                            mapServices.Add(ms);
                        }
                        #endregion

                        #region onstart - load

                        foreach (XmlNode serviceNode in doc.SelectNodes("MapServer/onstart/load/services/service"))
                        {
                            ServiceRequest serviceRequest = new ServiceRequest(
                                serviceNode.InnerText, String.Empty);

                            ServiceRequestContext context = new ServiceRequestContext(
                                mapServer,
                                null,
                                serviceRequest);

                            IServiceMap sm = mapServer[context];

                            /*
                            // Initalisierung...?!
                            sm.Display.iWidth = sm.Display.iHeight = 50;
                            IEnvelope env = null;
                            foreach (IDatasetElement dsElement in sm.MapElements)
                            {
                                if (dsElement != null && dsElement.Class is IFeatureClass)
                                {
                                    if (env == null)
                                        env = new Envelope(((IFeatureClass)dsElement.Class).Envelope);
                                    else
                                        env.Union(((IFeatureClass)dsElement.Class).Envelope);
                                }
                            }
                            sm.Display.ZoomTo(env);
                            sm.Render();
                             * */
                        }
                        #endregion

                        Console.WriteLine("config.xml loaded...");
                    }
                }
                catch (Exception ex)
                {
                    if (Functions.log_errors)
                        Logger.Log(loggingMethod.error, "LoadConfig - config.xml: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }

        internal static IMap LoadMap(string name, IServiceRequestContext context)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(ServicesPath);
                if (!di.Exists) di.Create();

                FileInfo fi = new FileInfo(ServicesPath + @"\" + name + ".mxl");
                if (fi.Exists)
                {
                    ServerMapDocument doc = new ServerMapDocument();
                    doc.LoadMapDocument(fi.FullName);

                    if (doc.Maps.Count == 1)
                    {
                        ApplyMetadata(doc.Maps[0] as Map);
                        if (!mapDocument.AddMap(doc.Maps[0]))
                            return null;

                        return doc.Maps[0];
                    }
                    return null;
                }
                fi = new FileInfo(ServicesPath + @"\" + name + ".svc");
                if (fi.Exists)
                {
                    XmlStream stream = new XmlStream("");
                    stream.ReadStream(fi.FullName);
                    IServiceableDataset sds = stream.Load("IServiceableDataset", null) as IServiceableDataset;
                    if (sds != null && sds.Datasets != null)
                    {
                        Map map = new Map();
                        map.Name = name;

                        foreach (IDataset dataset in sds.Datasets)
                        {
                            if (dataset is IRequestDependentDataset)
                            {
                                if (!((IRequestDependentDataset)dataset).Open(context)) return null;
                            }
                            else
                            {
                                if (!dataset.Open()) return null;
                            }
                            //map.AddDataset(dataset, 0);

                            foreach (IDatasetElement element in dataset.Elements)
                            {
                                if (element == null) continue;
                                ILayer layer = LayerFactory.Create(element.Class, element as ILayer);
                                if (layer == null) continue;

                                map.AddLayer(layer);

                                if (element.Class is IWebServiceClass)
                                {
                                    if (map.SpatialReference == null)
                                        map.SpatialReference = ((IWebServiceClass)element.Class).SpatialReference;

                                    foreach (IWebServiceTheme theme in ((IWebServiceClass)element.Class).Themes)
                                    {
                                        map.SetNewLayerID(theme);
                                    }
                                }
                                else if (element.Class is IFeatureClass && map.SpatialReference == null)
                                {
                                    map.SpatialReference = ((IFeatureClass)element.Class).SpatialReference;
                                }
                                else if (element.Class is IRasterClass && map.SpatialReference == null)
                                {
                                    map.SpatialReference = ((IRasterClass)element.Class).SpatialReference;
                                }
                            }
                        }
                        ApplyMetadata(map);

                        if (!mapDocument.AddMap(map))
                            return null;
                        return map;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }

            return null;
        }
        private static void ApplyMetadata(Map map)
        {
            try
            {
                if (map == null) return;
                FileInfo fi = new FileInfo(ServicesPath + @"\" + map.Name + ".meta");

                IServiceMap sMap = new ServiceMap(map, mapServer);
                XmlStream xmlStream;
                // 1. Bestehende Metadaten auf sds anwenden
                if (fi.Exists)
                {
                    xmlStream = new XmlStream("");
                    xmlStream.ReadStream(fi.FullName);
                    sMap.ReadMetadata(xmlStream);
                }
                // 2. Metadaten neu schreiben...
                xmlStream = new XmlStream("Metadata");
                sMap.WriteMetadata(xmlStream);

                if (map is Metadata)
                    ((Metadata)map).Providers = sMap.Providers;

                // Overriding: no good idea -> problem, if multiple instances do this -> killing the metadata file!!!
                //xmlStream.WriteStream(fi.FullName);
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }
        static public void SaveConfig(IMap map)
        {
            try
            {
                if (mapDocument == null) return;

                ServerMapDocument doc = new ServerMapDocument();
                if (!doc.AddMap(map))
                    return;

                XmlStream stream = new XmlStream("MapServer");
                stream.Save("MapDocument", doc);

                stream.WriteStream(ServicesPath + @"\" + map.Name + ".mxl");
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }

        static public void SaveServiceableDataset(IServiceableDataset sds, string name)
        {
            try
            {
                if (sds != null)
                {
                    XmlStream stream = new XmlStream("MapServer");
                    stream.Save("IServiceableDataset", sds);

                    stream.WriteStream(ServicesPath + @"\" + name + ".svc");

                    if (sds is IMetadata)
                    {
                        stream = new XmlStream("Metadata");
                        ((IMetadata)sds).WriteMetadata(stream);
                        stream.WriteStream(ServicesPath + @"\" + name + ".svc.meta");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }

        static public void SaveServiceCollection(string name, XmlStream stream)
        {
            stream.WriteStream(ServicesPath + @"\" + name + ".scl");
        }

        static public bool RemoveConfig(string mapName)
        {
            try
            {
                FileInfo fi = new FileInfo(ServicesPath + @"\" + mapName + ".mxl");
                if (fi.Exists) fi.Delete();
                fi = new FileInfo(ServicesPath + @"\" + mapName + ".svc");
                if (fi.Exists) fi.Delete();

                return true;
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
                return false;
            }
        }

        static internal void mapDocument_MapAdded(IMap map)
        {
            try
            {
                Console.WriteLine("Map Added:" + map.Name);

                foreach (IDatasetElement element in map.MapElements)
                {
                    Console.WriteLine("     >> " + element.Title + " added");
                }
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }
        static internal void mapDocument_MapDeleted(IMap map)
        {
            try
            {
                Console.WriteLine("Map Removed: " + map.Name);
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "LoadConfig: " + ex.Message);
            }
        }

        static internal int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                MapServerConfig.ServerConfig serverConfig = MapServerConfig.ServerByInstancePort(_port);
                ServicesPath = gView.Framework.system.SystemVariables.MyCommonApplicationData + @"\mapServer\Services\" + serverConfig.Port;

                try
                {
                    MapServerConfig.ServerConfig.InstanceConfig InstanceConfig = MapServerConfig.InstanceByInstancePort(value);
                    if (serverConfig!=null && InstanceConfig != null)
                    {
                        OutputPath = serverConfig.OutputPath.Trim();
                        OutputUrl = serverConfig.OutputUrl.Trim();
                        TileCachePath = serverConfig.TileCachePath.Trim();

                        Functions.MaxThreads = InstanceConfig.MaxThreads;
                        Functions.QueueLength = InstanceConfig.MaxQueueLength;

                        Logger.Log(loggingMethod.request, "Output Path: '" + OutputPath + "'");
                        Logger.Log(loggingMethod.request, "Output Url : '" + OutputUrl + "'");
                    }
                    threadQueue = new ThreadQueue<IServiceRequestContext>(Functions.MaxThreads, Functions.QueueLength);
                }
                catch (Exception ex)
                {
                    if (Functions.log_errors)
                    {
                        Logger.Log(loggingMethod.error, "IMS.Port(set): " + ex.Message + "\r\n" + ex.StackTrace);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 256;
            //System.Net.ServicePointManager.MaxServicePoints = 256;
            System.Net.ServicePointManager.CertificatePolicy = new gView.Framework.Web.SimpleHttpsPolicy();

            gView.Framework.system.gViewEnvironment.UserInteractive = false;

            //int x = 0;
            //x = 1 / x;
            try
            {
                //Thread thread = new Thread(new ThreadStart(StartThread2));
                //thread.Start();

                //gView.Framework.system.SystemVariables.ApplicationDirectory = gView.Framework.system.SystemVariables.RegistryApplicationDirectory;
                Logger.Log(loggingMethod.request, "Service EXE started...");

                bool console = false;
                int port = -1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-c") console = true;
                    if (args[i] == "-port") port = int.Parse(args[i + 1]);
                }
                //port = 8001;
                //console = true;

                Logger.Log(loggingMethod.request, "Current Directory: " + System.Environment.CurrentDirectory);
                //Logger.Log(loggingMethod.request, "Check license...");
                //myLicense = new License(false);

                //Logger.Log(loggingMethod.request, "License Info********");
                //Logger.Log(loggingMethod.request, "ProductID: " + myLicense.ProductID);
                //Logger.Log(loggingMethod.request, "InstallationDate: " + SystemVariables.InstallationTime.ToShortDateString() + " " + SystemVariables.InstallationTime.ToLongTimeString());
                //Logger.Log(loggingMethod.request, "LicenseFile: " + myLicense.LicenseFile);
                //Logger.Log(loggingMethod.request, "LicenseFileExists: " + myLicense.LicenseFileExists.ToString());
                //if (myLicense.LicenseComponents != null)
                //{
                //    Logger.Log(loggingMethod.request, "Components:");
                //    foreach (string component in myLicense.LicenseComponents)
                //        Logger.Log(loggingMethod.request, "   " + component);
                //}

                //LicenseTypes licType = IMS.myLicense.ComponentLicenseType("gview.Server;gview.PersonalMapServer");
                //if (licType == LicenseTypes.Unknown ||
                //    licType == LicenseTypes.Expired)
                //{
                //    Logger.Log(loggingMethod.error, "Server is not licensed...");
                //    return;
                //}
                //Logger.Log(loggingMethod.request, "********************");

                // Interpreter suchen...
                Logger.Log(loggingMethod.request, "Register request interpreters...");
                mapServer = new MapServer(port);
                /*
                switch (mapServer.LicType)
                {
                    case MapServer.ServerLicType.Unknown:
                        Logger.Log(loggingMethod.error, "Unkown License");
                        if (console)
                        {
                            Console.WriteLine("Unkown License...");
#if(DEBUG)
                            Console.ReadLine();
#endif
                        }
                        return;
                    case MapServer.ServerLicType.Express:
                    case MapServer.ServerLicType.Private:
                        System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("gview.mapserver");
                        if (procs.Length > 1)
                        {
                            Logger.Log(loggingMethod.error, "Your server license type '" + mapServer.LicType.ToString() + "' allows only one server instance!");
                            if (console)
                            {
                                Console.WriteLine("Your server license type '" + mapServer.LicType.ToString() + "' allows only one server instance!");
#if(DEBUG)
                                Console.ReadLine();
#endif
                            }
                            return;
                        }
                        break;
                }
                 * */
                PlugInManager compMan = new PlugInManager();
                foreach (XmlNode interpreterNode in compMan.GetPluginNodes(Plugins.Type.IServiceRequestInterpreter))
                {
                    IServiceRequestInterpreter interpreter = compMan.CreateInstance(interpreterNode) as IServiceRequestInterpreter;
                    if (interpreter == null) continue;
                    interpreter.OnCreate(mapServer);
                    _interpreters.Add(interpreter);
                }

                try
                {
                    SetConsoleTitle("gView.MapServer.Instance." + port);
                }
                catch { }
#if(!DEBUG)
            if (console)
            {
                //ImageService service = new ImageService();
                //service.Start(args);

                ServerProcess process = new ServerProcess(port, false);
                Console.WriteLine("Server is running...");
                Console.ReadLine();

                process.Stop();
                //service.Stop();
            }
            else
            {
                if (port != -1)
                {
                    Console.WriteLine("Start Server On Port: " + port);
                    ServerProcess process = new ServerProcess(port, true);
                }
                
                else
                {
                    System.ServiceProcess.ServiceBase[] ServicesToRun;
                    ServicesToRun = new System.ServiceProcess.ServiceBase[] { new ImageService() };
                    System.ServiceProcess.ServiceBase.Run(ServicesToRun);
                }
            }
#else
                ServerProcess process = new ServerProcess((port != -1) ? port : 8001, false);
                Console.WriteLine("Server Instance is running...");
                Console.ReadLine();

                process.Stop();
#endif
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, ex.Message);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string msg = String.Empty;
            msg += "UnhandledException:\n";
            if (e.ExceptionObject is Exception)
            {
                msg += "Exception:" + ((Exception)e.ExceptionObject).Message + "\n";
                msg += "Stacktrace:" + ((Exception)e.ExceptionObject).StackTrace + "\n";
            }
            Logger.Log(loggingMethod.error, msg);
            //Console.WriteLine("UnhandledException:");
            //if (e.ExceptionObject is Exception)
            //{
            //    Console.WriteLine("Exception:" + ((Exception)e.ExceptionObject).Message);
            //    Console.WriteLine("Source:" + ((Exception)e.ExceptionObject).Source);
            //    Console.WriteLine("Stacktrace:" + ((Exception)e.ExceptionObject).StackTrace);
            //}
            System.Environment.Exit(1);
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool SetConsoleTitle(string text);
    }
}
