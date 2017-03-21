using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using gView.Framework.system;

namespace gView.MapServer.Tasker
{
    static class Program
    {
        public static TaskerService Tasker { get; private set; }

        static void Main(string[] args)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 256;
            System.Net.ServicePointManager.MaxServicePoints = 256;
            //System.Net.ServicePointManager.CertificatePolicy = new gView.Framework.Web.SimpleHttpsPolicy();

            bool console = false, startInstances = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c") console = true;
                if (args[i] == "-debug") startInstances = false;
            }


            if (console)
            {
                TaskerService service = Program.Tasker = new TaskerService(startInstances);
                service.StartFromConsole();

                System.Console.WriteLine("Tasker is running...");
                System.Console.ReadLine();

                service.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun;

                ServicesToRun = new ServiceBase[] { Program.Tasker = new TaskerService() };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}