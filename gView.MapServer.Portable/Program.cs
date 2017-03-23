using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.MapServer.Portable
{
    static class Program
    {
        public static TaskerService Tasker { get; private set; }

        static void Main(string[] args)
        {
            #region Check for exiting Instance of this Process

            var currentProcess = Process.GetCurrentProcess();
            Console.WriteLine("Current Process: " + currentProcess.ProcessName + " [" + currentProcess.Id + "]");
            foreach(var process in Process.GetProcesses())
            {
                if(process.ProcessName.ToLower()==currentProcess.ProcessName.ToLower() &&
                    process.Id!=currentProcess.Id)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Process already running with id " + process.Id);
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(3000);
                    return;
                }
            }

            #endregion

            System.Net.ServicePointManager.DefaultConnectionLimit = 256;
            System.Net.ServicePointManager.MaxServicePoints = 256;
            //System.Net.ServicePointManager.CertificatePolicy = new gView.Framework.Web.SimpleHttpsPolicy();

            bool startInstances = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-debug") startInstances = false;
            }


            using (TaskerService service = Program.Tasker = new TaskerService(startInstances))
            {
                service.StartFromConsole();

                System.Console.ReadLine();

                Console.WriteLine("Stopping MapServer...");
            }
        }
    }
}
