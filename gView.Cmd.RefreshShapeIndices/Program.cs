using gView.DataSources.Shape;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.RefreshShapeIndices
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = String.Empty;
            bool delExisting = false;

            gView.Framework.system.gViewEnvironment.UserInteractive = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-path")
                {
                    path = args[++i];
                }
                else if (args[i] == "-delexisting")
                {
                    delExisting = true;
                }
            }

            if (String.IsNullOrEmpty(path))
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.RefreshShapeIndices -path <shape file path>");
                Console.WriteLine("                     [-delexisting]");
                return;
            }

            if (delExisting)
            {
                foreach (FileInfo fi in new DirectoryInfo(path).GetFiles("*.idx"))
                {
                    Console.WriteLine("delete " + fi.FullName + "...");
                    fi.Delete();
                }
            }

            ShapeDataset ds = new ShapeDataset();
            ds.ConnectionString = path;

            if (!ds.Open())
            {
                Console.WriteLine("ERROR: " + ds.lastErrorMsg);
                return;
            }

            foreach (FileInfo fi in new DirectoryInfo(path).GetFiles("*.shp"))
            {
                var element = ds[fi.Name];
                if (element == null)
                    continue;

                Console.WriteLine("found shape: " + element.Class.Name);
            }

            Console.WriteLine("finished");
        }
    }
}
