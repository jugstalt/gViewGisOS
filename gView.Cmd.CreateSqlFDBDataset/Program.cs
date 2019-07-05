using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.DataSources.Fdb.MSSql;

namespace CreateSqlFDBDataset
{
    class Program
    {
        static int Main(string[] args)
        {
            string server = "", database = "", uid = "", pwd = "", dataset = "", imageSpace="database";
            string connectionString = "";
            bool delExisting = true;
            bool imagedataset = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-server")
                {
                    server = args[++i];
                }
                if (args[i] == "-uid")
                {
                    uid = args[++i];
                }
                if (args[i] == "-pwd")
                {
                    pwd = args[++i];
                }
                if (args[i] == "-database")
                {
                    database = args[++i];
                }
                if (args[i] == "-ds")
                {
                    dataset = args[++i];
                }
                if (args[i] == "-imagedataset")
                {
                    imagedataset = true;
                }
                if (args[i] == "-imagespace")
                {
                    imageSpace = args[++i];
                }
                if (args[i] == "-connectionstring")
                {
                    connectionString = args[++i];
                }
            }

            if (((server == "" && database == "") && connectionString == "") || dataset == "")
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.CreateSqlFDBDataset -server <Filename> -database <Database> OR -connectionstring <ConnectionString>");
                Console.WriteLine("                    -ds <Datasetname>");
                Console.WriteLine("                   [-uid <User> -pwd <Password> -imagedataset -imagespace <Image Space Directory (Database)>]");
                return 1;
            }

            try
            {
                if (connectionString == "")
                {
                    connectionString = "server=" + server + ";database=" + database;
                    if (uid != "")
                    {
                        connectionString += ";uid=" + uid + ";pwd=" + pwd;
                    }
                    else
                    {
                        connectionString += ";Trusted_Connection=True";
                    }
                }
                SqlFDB fdb = new SqlFDB();
                if (!fdb.Open(connectionString))
                {
                    Console.WriteLine("\n\nERROR: " + fdb.lastErrorMsg);
                    return 1;
                }

                //SpatialReference sRef = new SpatialReference();
                ISpatialIndexDef sIndexDef = new gViewSpatialIndexDef();

                if (imagedataset)
                {
                    if (fdb.CreateImageDataset(dataset, null, sIndexDef, imageSpace, null) < 1)
                    {
                        fdb.Dispose();
                        Console.WriteLine("\n\nERROR: " + fdb.lastErrorMsg);
                        return 1;
                    }
                }
                else
                {
                    if (fdb.CreateDataset(dataset, null, sIndexDef) < 1)
                    {
                        fdb.Dispose();
                        Console.WriteLine("\n\nERROR: " + fdb.lastErrorMsg);
                        return 1;
                    }
                }

                fdb.Dispose();
                Console.WriteLine(server + @"\" + database + ": Dataset " + dataset + " created...");

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
