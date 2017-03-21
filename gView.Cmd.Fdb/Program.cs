using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// gview.Cmd.Fdb.exe -connstr "c:\temp\test.fdb" -guid "36DEB6AC-EA0C-4B37-91F1-B2E397351555" -c createindex -fc [fcName] -fields "Field1,Field1"
// gview.Cmd.Fdb.exe -connstr "c:\temp\test.fdb" -guid "36DEB6AC-EA0C-4B37-91F1-B2E397351555" -c dropindex -name [IndexName]

namespace gView.Cmd.Fdb
{
    class Program
    {
        enum Command
        {
            Unknown = 0,
            CreateIndex = 1,
            DropIndex = 2
        }

        static void Main(string[] args)
        {
            string connString=String.Empty, fcName=String.Empty, fields=String.Empty, name=String.Empty;
            Guid dsGuid=new Guid();
            Command command = Command.Unknown;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-connstr")
                    connString = args[++i];

                else if (args[i] == "-guid")
                    dsGuid = new Guid(args[++i]);

                else if (args[i] == "-fc")
                    fcName = args[++i];

                else if (args[i] == "-fields")
                    fields = args[++i];

                else if (args[i] == "-name")
                    name = args[++i];

                else if (args[i] == "-c")
                {
                    switch (args[++i].ToLower())
                    {
                        case "createindex":
                            command = Command.CreateIndex;
                            break;
                        case "dropindex":
                            command = Command.DropIndex;
                            break;
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(connString) || dsGuid.Equals(new Guid()) ||
                command == Command.Unknown)
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.Fdb -connstr <ConnectionString> -guid <DatasetGuid>");
                Console.WriteLine("              -c <CreateIndex|DropIndex>");
                return;
            }

            IFeatureDataset dataset;

            PlugInManager compMan = new PlugInManager();
            object comp = compMan.CreateInstance(dsGuid);

            if (!(comp is IFeatureDataset))
            {
                Console.WriteLine("Component with GUID '" + dsGuid.ToString() + "' is not a feature dataset...");
                return;
            }

            dataset = (IFeatureDataset)comp;

            if (!(dataset.Database is AccessFDB))
            {
                Console.WriteLine("Component dataset is not a gView Feature-Database...");
                return;
            }

            AccessFDB fdb = (AccessFDB)dataset.Database;
            if (!fdb.Open(connString))
            {
                Console.WriteLine("Can't open database");
                return;
            }

            if (command == Command.CreateIndex)
            {
                #region Create Index

                if (String.IsNullOrEmpty(fcName) || String.IsNullOrEmpty(fields))
                {
                    Console.WriteLine("USAGE:");
                    Console.WriteLine("gView.Cmd.Fdb -connstr <ConnectionString> -guid <DatasetGuid>");
                    Console.WriteLine("              -c <CreateIndex>");
                    Console.WriteLine("              -fc <FeatureClassName> -fields <Fields>"); 
                    return;
                }

                string indexName = "IDX_FC_" + fcName + "_" + fields.Replace(",", "_").Replace(" ", "");
                try
                {
                    Console.WriteLine("Try drop index...");
                    fdb.DropIndex(indexName);
                }
                catch { }

                Console.WriteLine("Try create index...");
                if (!fdb.CreateIndex(indexName,
                    "FC_" + fcName, fields, false))
                {
                    Console.WriteLine("ERROR:");
                    Console.WriteLine(fdb.lastErrorMsg);
                    return;
                }

                Console.WriteLine("Index created...");
                return;

                #endregion
            }
            else if (command == Command.DropIndex)
            {
                #region Drop Index

                if (String.IsNullOrEmpty(name))
                {
                    Console.WriteLine("USAGE:");
                    Console.WriteLine("gView.Cmd.Fdb -connstr <ConnectionString> -guid <DatasetGuid>");
                    Console.WriteLine("              -c <DropIndex>");
                    Console.WriteLine("              -name <IndexName>"); 
                    return;
                }

                Console.WriteLine("Try drop index...");
                if (!fdb.DropIndex(name))
                {
                    Console.WriteLine("ERROR:");
                    Console.WriteLine(fdb.lastErrorMsg);
                    return;
                }

                Console.WriteLine("Index dropped...");
                return;

                #endregion
            }
        }
    }
}
