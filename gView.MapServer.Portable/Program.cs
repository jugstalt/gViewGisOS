using System;
using System.Collections.Generic;
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
