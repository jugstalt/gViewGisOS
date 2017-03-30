using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Cmd.ExecuteSerializable
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string assemblyName = String.Empty;
                string instanceType = String.Empty;
                string configFile = String.Empty;

                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (args[i] == "-assembly")
                    {
                        assemblyName = args[++i];
                    }
                    if (args[i] == "-instance")
                    {
                        instanceType = args[++i];
                    }
                    if (args[i] == "-config")
                    {
                        configFile = args[++i];
                    }
                }

                if (String.IsNullOrWhiteSpace(assemblyName) || String.IsNullOrWhiteSpace(instanceType) || String.IsNullOrWhiteSpace(configFile))
                {
                    Console.WriteLine("USAGE:");
                    Console.WriteLine("gView.Cmd.ExecuteSerializable -config <Config Filename>");
                    Console.WriteLine("                              -assembly <assembly name>");
                    Console.WriteLine("                              -instance <instance name>");
                    return;
                }

                var assembly = Assembly.LoadFrom(SystemVariables.ApplicationDirectory + @"\" + assemblyName);
                var instance = assembly.CreateInstance(instanceType, true);

                if (instance == null)
                    throw new Exception("Type not found: " + instanceType);

                if (instance is Form)
                    ((Form)instance).Show();

                ProgressReporterEvent reportDelege = new ProgressReporterEvent(ReportProgress);

                if (instance is ISerializableExecute)
                {
                    ((ISerializableExecute)instance).DeserializeObject(System.IO.File.ReadAllText(configFile));
                    ((ISerializableExecute)instance).Execute(reportDelege);

                    Console.WriteLine();
                    Console.WriteLine("finished");
                }
                else
                {
                    throw new Exception("Type " + instanceType + " do not implement ISerializableExecute");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        static string _reportProgressTitle = String.Empty;
        static void ReportProgress(ProgressReport report)
        {
            if (report.Message!=_reportProgressTitle)
            {
                _reportProgressTitle = report.Message;
                Console.WriteLine();
                Console.WriteLine(report.Message);
            }
            else
            {
                Console.Write("..." + report.featurePos);
            }
        }
    }
}
