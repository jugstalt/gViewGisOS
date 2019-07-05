using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.DataSources.Fdb.MSAccess;

namespace CreateAccessFDBDataset
{
    class Program
    {
        static int Main(string[] args)
        {
            string mdb = "",dataset="";
            bool delExisting = true;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-mdb")
                {
                    mdb = args[++i];
                }
                if (args[i] == "-ds")
                {
                    dataset = args[++i];
                }
                else if (args[i] == "-delexisting")
                {
                    try { delExisting = Convert.ToBoolean(args[++i]); }
                    catch { }
                }
            }

            if (mdb == "" || dataset=="")
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.CreateAccessFDBDataset -mdb <Filename> -ds <Datasetname>");
                Console.WriteLine("                      [-delexisting <true|false>]");
                return 1;
            }

            try
            {
                AccessFDB fdb = new AccessFDB();
                if (!fdb.Open(mdb))
                {
                    Console.WriteLine("\n\nERROR: " + fdb.lastErrorMsg);
                    return 1;
                }

                //SpatialReference sRef = new SpatialReference();
                
                if (fdb.CreateDataset(dataset,null)<1)
                {
                    Console.WriteLine("\n\nERROR: " + fdb.lastErrorMsg);
                    fdb.Dispose();
                    return 1;
                }
                fdb.Dispose();
                Console.WriteLine(mdb + ": Dataset " + dataset + " created...");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nERROR: " + ex.Message);

                return 1;
            }
        }
    }
}
