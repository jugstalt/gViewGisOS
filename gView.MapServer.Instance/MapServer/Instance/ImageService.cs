using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Runtime.Remoting.Channels; //To support and handle Channel and channel sinks
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Http; //For HTTP channel
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using gView.MapServer;
using gView.Framework.system;

namespace gView.MapServer.Instance
{
    public partial class ImageService : ServiceBase
    {
        private List<Process> _procs = new List<Process>();

        public ImageService()
        {
            try
            {
                InitializeComponent();

                System.Environment.CurrentDirectory = gView.Framework.system.SystemVariables.ApplicationDirectory;

                // init globals....
                IMS dummy = new IMS();
                gView.Framework.system.SystemVariables dummy2 = new gView.Framework.system.SystemVariables();
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "Service Create: " + ex.Message);
            }
        }

        private Thread _delThread = null;
        private HttpServerChannel _channel;
        protected override void OnStart(string[] args)
        {
            try
            {
                //XmlDocument doc = new XmlDocument();
                //doc.Load(gView.Framework.system.SystemVariables.ApplicationDirectory + @"\mapServer\Processes.xml");

                bool first = true;
                //foreach (XmlNode procNode in doc.SelectNodes("processes/process[@port]"))
                for(int i=0;i<MapServerConfig.ServerCount;i++)
                {
                    MapServerConfig.ServerConfig procConfig = MapServerConfig.Server(i);
                    if (procConfig == null) continue;

                    if (first)
                    {
                        //RegisterPort(int.Parse(procNode.Attributes["port"].Value));
                        RegisterPort(procConfig.Port);
                        first = false;
                    }
                    else
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = gView.Framework.system.SystemVariables.ApplicationDirectory + @"\gView.MapServer.exe";
                        //proc.StartInfo.Arguments = "-port " + procNode.Attributes["port"].Value;
                        proc.StartInfo.Arguments = "-port " + procConfig.Port;
                        proc.StartInfo.UseShellExecute = false;

                        proc.Start();
                        _procs.Add(proc);
                    }
                }

                DeleteImageThread del = new DeleteImageThread(Functions.outputPath);
                _delThread = new Thread(new ThreadStart(del.Run));
                _delThread.Start();
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "Service OnStart: " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {
                IMS.mapDocument.MapAdded -= new gView.Framework.UI.MapAddedEvent(IMS.mapDocument_MapAdded);
                IMS.mapDocument.MapDeleted -= new gView.Framework.UI.MapDeletedEvent(IMS.mapDocument_MapDeleted);

                ChannelServices.UnregisterChannel(_channel);

                foreach (Process proc in _procs)
                {
                    proc.Kill();
                }

                if (_delThread != null)
                {
                    while (_delThread.IsAlive)
                    {
                        _delThread.Abort();
                        Thread.Sleep(100);
                    }
                    _delThread = null;
                }
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "Service OnStop: " + ex.Message);
            }
        }

        private void RegisterPort(int port)
        {
            IMS.ServicesPath = gView.Framework.system.SystemVariables.ApplicationDirectory + @"\mapServer\Services\" + port;
            IMS.Port = port;

            IMS.mapDocument.MapAdded += new gView.Framework.UI.MapAddedEvent(IMS.mapDocument_MapAdded);
            IMS.mapDocument.MapDeleted += new gView.Framework.UI.MapDeletedEvent(IMS.mapDocument_MapDeleted);
            IMS.LoadConfigAsync();

            _channel = new HttpServerChannel(port);
            ChannelServices.RegisterChannel(_channel); //Register channel

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MapServerInstanceType),
                "MapServer",
                WellKnownObjectMode.Singleton);
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }
        public void Stop()
        {
            OnStop();
        }
    }
}
