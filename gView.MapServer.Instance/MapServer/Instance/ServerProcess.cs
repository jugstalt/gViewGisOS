using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Channels; //To support and handle Channel and channel sinks
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Http; //For HTTP channel
using gView.MapServer;
using gView.Framework.system;
using System.ServiceModel;

namespace gView.MapServer.Instance
{
    internal class ServerProcess
    {
        public ServerProcess(int port, bool wait)
        {
            try
            {
                IMS.Port=port;
                System.Environment.CurrentDirectory = gView.Framework.system.SystemVariables.RegistryApplicationDirectory;

                // init globals....
                IMS dummy = new IMS();
                gView.Framework.system.SystemVariables dummy2 = new gView.Framework.system.SystemVariables();

                Start();

                if (wait)
                {
                    ManualResetEvent resetEvent = new ManualResetEvent(false);
                    resetEvent.WaitOne();

                    Stop();
                }
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "Service Create: " + ex.Message);
            }
        }


        private static ServiceHost _host;
        private void Start_WCF()
        {
            _host = new ServiceHost(typeof(MapServerInstanceType), new Uri("http://localhost:" + IMS.Port + "/MapServer"));
            _host.Open();
        }

        private HttpServerChannel _channel;
        public void Start()
        {
            try
            {
                IMS.mapDocument.MapAdded += new gView.Framework.UI.MapAddedEvent(IMS.mapDocument_MapAdded);
                IMS.mapDocument.MapDeleted += new gView.Framework.UI.MapDeletedEvent(IMS.mapDocument_MapDeleted);
                IMS.LoadConfigAsync();

                _channel = new HttpServerChannel(IMS.Port);
                ChannelServices.RegisterChannel(_channel); //Register channel

                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(MapServerInstanceType),
                    "MapServer",
                    WellKnownObjectMode.Singleton);

                //Console.WriteLine("Server ON at port number:"+Functions.Port);
                //Console.WriteLine("Please press enter to stop the server.");
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "Server Start: " + ex.Message);

                System.Threading.Thread.Sleep(3000);
                System.Environment.Exit(1);
            }
        }

        public void Stop()
        {
            try
            {
                IMS.mapDocument.MapAdded -= new gView.Framework.UI.MapAddedEvent(IMS.mapDocument_MapAdded);
                IMS.mapDocument.MapDeleted -= new gView.Framework.UI.MapDeletedEvent(IMS.mapDocument_MapDeleted);

                ChannelServices.UnregisterChannel(_channel);
            }
            catch (Exception ex)
            {
                if (Functions.log_errors)
                    Logger.Log(loggingMethod.error, "Server Stop: " + ex.Message);
            }
        }
    }
}
