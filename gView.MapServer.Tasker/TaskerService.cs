using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using gView.Framework.system;
using System.Threading;
using System.IO;
using System.ServiceModel;

namespace gView.MapServer.Tasker
{
    public partial class TaskerService : ServiceBase
    {
        private bool _running = false, _startInstances;
        private Thread _delThread = null;
        private List<ServiceHost> _hosts = new List<ServiceHost>();

        public TaskerService()
            : this(true)
        {
        }
        public TaskerService(bool startInstances)
        {
            InitializeComponent();

            _startInstances = startInstances;
            //_host = new ServiceHost(typeof(Service.TaskerServiceType), new Uri("http://localhost:" + _port));
        }

        public void StartFromConsole()
        {
            OnStart(new string[] { });
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                KillProcesses();

                for (int s = 0; s < MapServerConfig.ServerCount; s++)
                {
                    MapServerConfig.ServerConfig serverConfig = MapServerConfig.Server(s);

                    if (_startInstances)
                    {
                        Logger.Log(loggingMethod.request, "Start Instances...");
                        _running = true;
                        for (int i = 0; i < serverConfig.Instances.Length; i++)
                        {
                            MapServerConfig.ServerConfig.InstanceConfig instanceConfig = serverConfig.Instances[i];
                            if (instanceConfig == null || instanceConfig.Port <= 0) continue;

                            Thread thread = new Thread(new ParameterizedThreadStart(ProcessMonitor));
                            thread.Start(instanceConfig.Port);
                        }
                    }

                    Service.TaskerServiceType serviceType=new Service.TaskerServiceType();
                    serviceType.Init(serverConfig.Port);
                    ServiceHost host = new ServiceHost(/*typeof(Service.TaskerServiceType)*/serviceType, new Uri("http://localhost:" + serverConfig.Port));

                    try
                    {
                        if (host.Description.Endpoints.Count == 1 &&
                            host.Description.Endpoints[0].Binding is System.ServiceModel.WebHttpBinding)
                        {
                            System.ServiceModel.WebHttpBinding binding = (System.ServiceModel.WebHttpBinding)host.Description.Endpoints[0].Binding;
                            binding.MaxReceivedMessageSize = int.MaxValue;
                        }
                    }
                    catch { }

                    host.Open();
                    _hosts.Add(host);
                }
                DeleteImageThread del = new DeleteImageThread();
                _delThread = new Thread(new ThreadStart(del.Run));
                _delThread.Start();

            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        protected override void OnStop()
        {
            try
            {
                Logger.Log(loggingMethod.request, "Stop Service...");

                KillProcesses();

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
                Logger.Log(loggingMethod.error, "Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void ProcessMonitor(object p)
        {
            try
            {
                if (p == null || p.GetType() != typeof(int)) return;
                int port = (int)p;

                bool first = true;
                while (_running)
                {
                    Logger.Log(loggingMethod.request,
                        ((first) ? "Start" : "Recircle") +
                        " map server on port " + port.ToString());

                    Process proc = new Process();
                    proc.StartInfo.FileName = gView.Framework.system.SystemVariables.ApplicationDirectory + @"\gView.MapServer.Instance.exe";
                    proc.StartInfo.Arguments = "-port " + port.ToString();
                    proc.StartInfo.UseShellExecute = false;

                    proc.Start();
                    proc.WaitForExit();

                    Logger.Log(loggingMethod.request,
                        "Map server on port " + port.ToString() + " killed");
                    first = false;

                    try
                    {
                        proc.Kill();
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(loggingMethod.error, "Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void KillProcesses()
        {
            foreach (ServiceHost host in _hosts)
            {
                host.Close();
            }
            _hosts.Clear();

            _running = false;
            foreach (Process proc in Process.GetProcessesByName("gView.MapServer.Instance"))
            {
                try
                {
                    proc.Kill();
                }
                catch { }
            }
        }
    }

    class DeleteImageThread
    {
        public DeleteImageThread()
        {
        }

        public void Run()
        {
            if (MapServerConfig.RemoveInterval <= 0) return;

            while (true)
            {
                Thread.Sleep(10000);
                Delete("*.jpg");
                Delete("*.png");
                Delete("*.gif");
                Delete("*.tif");
                Delete("*.tiff");
                Delete("*.bmp");
                Delete("*.sld");
            }
        }

        private void Delete(string filter)
        {
            try
            {
                string last = String.Empty;
                for (int i = 0; i < MapServerConfig.ServerCount; i++)
                {
                    MapServerConfig.ServerConfig procConfig = MapServerConfig.Server(i);
                    if (procConfig == null || procConfig.OutputPath == last) continue;
                    last = procConfig.OutputPath;

                    DirectoryInfo di = new DirectoryInfo(procConfig.OutputPath);

                    foreach (FileInfo fi in di.GetFiles(filter))
                    {
                        TimeSpan ts = DateTime.Now - fi.CreationTime;
                        if (ts.TotalMinutes >= MapServerConfig.RemoveInterval)
                        {
                            try
                            {
                                fi.Delete();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }

        }
    }
}
