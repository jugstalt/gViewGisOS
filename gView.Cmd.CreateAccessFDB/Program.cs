using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.DataSources.Fdb.MSAccess;

namespace CreateAccessFDB
{
    class Program
    {
        static void Main(string[] args)
        {
            string mdb = "";
            bool delExisting = true;
            for (int i = 0; i < args.Length-1; i++)
            {
                if (args[i] == "-mdb")
                {
                    mdb = args[++i];
                }
                else if (args[i] == "-delexisting")
                {
                    try { delExisting = Convert.ToBoolean(args[++i]); }
                    catch { }
                }
            }

            if (mdb == "")
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.CreateAccessFDB -mdb <dest. Filename>");
                Console.WriteLine("                     [-delexisting <true|false>]");
                return;
            }

            try
            {
                FileInfo fi = new FileInfo(mdb);
                if (!fi.Exists)
                {
                    if (!fi.Directory.Exists) fi.Directory.Create();
                }
                else
                {
                    if (delExisting)
                    {
                        fi.Delete();
                    }
                    else
                    {
                        Console.WriteLine("\n\nERROR: alread exists!");
                    }
                }

                AccessFDB fdb = new AccessFDB();
                if (!fdb.Create(mdb))
                {
                    Console.WriteLine("\n\nERROR: " + fdb.lastErrorMsg);
                    return;
                }
                Console.WriteLine(mdb + " created...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nERROR: " + ex.Message);
            }
        }
    }
}
