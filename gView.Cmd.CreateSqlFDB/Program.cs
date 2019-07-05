using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.system;
using gView.DataSources.Fdb.MSSql;

namespace CreateSqlFDB
{
    class Program
    {
        static int Main(string[] args)
        {
            string server = "", uid = "", pwd = "", database = "", mdf = String.Empty;
            UserData userData = new UserData();

            for (int i = 0; i < args.Length - 1; i++)
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
                if (args[i] == "-mdf")
                {
                    mdf = args[++i];
                }
                if (args[i].StartsWith("-_"))
                {
                    userData.SetUserData(args[i].Substring(2, args[i].Length - 2), args[++i]);
                }
            }

            if (server == "" || database == "")
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.CreateSqlFDB -server <SQL Server Instance>");
                Console.WriteLine("             -database <Database Name>");
                Console.WriteLine("            [-uid <User>]");
                Console.WriteLine("            [-pwd <Password>]");
                Console.WriteLine("            [-mdf <mdf_file_path>");
                Console.WriteLine("            [-_Parametername <parameter>]");
                Console.WriteLine("Example:");
                Console.WriteLine("CreateSqlFDB -server localhost -database NEW_FDB -_filename c:\\myDatabase.mdf -_size 300 -_filegrowth 50");
                return 1;
            }

            // Path setzen, damit in fdb.Create die createdatabase.sql gefunden wird...
            /*
            if (SystemVariables.RegistryApplicationDirectory != "")
            {
                SystemVariables.ApplicationDirectory = SystemVariables.RegistryApplicationDirectory;
            }
            */

            string connectionString = "server=" + server;
            if (uid != "")
            {
                connectionString += ";uid=" + uid + ";pwd=" + pwd;
            }
            else
            {
                connectionString += ";Trusted_Connection=True";
            }

            SqlFDB fdb = new SqlFDB();
            if (!fdb.Open(connectionString))
            {
                Console.WriteLine("ERROR :" + fdb.lastErrorMsg);
                fdb.Dispose();
                return 1;
            }

            if (!String.IsNullOrEmpty(mdf))
            {
                if (!fdb.Create(database, mdf))
                {
                    Console.WriteLine("ERROR :" + fdb.lastErrorMsg);
                    fdb.Dispose();
                    return 1;
                }
            }
            else
            {
                if (userData.UserDataTypes.Length > 0)
                {
                    Console.WriteLine("CREATE DATABASE "+database+" ON PRIMARY (NAME="+database+",");
                    foreach (string type in userData.UserDataTypes)
                    {
                        Console.WriteLine(type + "=");
                        if (type.ToLower() == "filename")
                            Console.WriteLine("'" + userData.GetUserData(type) + "'");
                        else
                            Console.WriteLine(userData.GetUserData(type));
                    }
                    Console.WriteLine(")");
                }
                if (!fdb.Create(database, userData))
                {
                    Console.WriteLine("ERROR :" + fdb.lastErrorMsg);
                    fdb.Dispose();
                    return 1;
                }
            }
            fdb.Dispose();
            Console.WriteLine("...done");

            return 0;
        }
    }
}
