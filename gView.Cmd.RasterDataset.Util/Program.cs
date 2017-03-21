using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.IO;
using gView.Framework.Data;
using gView.Framework.FDB;
using System.IO;
using System.Text.RegularExpressions;
using gView.Framework.system;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.DataSources.Fdb.SQLite;
using gView.DataSources.Fdb.UI;

namespace gView.RasterDataset.Util
{
    class Program
    {
        enum jobs { add, truncate, removeUnexisting, unknown };
        static jobs job = jobs.unknown;
        static string connectinString = String.Empty;
        static string dbType = "sql", provider = "none";
        static string fileName = String.Empty, rootPath = String.Empty, Filters = String.Empty;
        static bool continueOnError = true;

        static void Main(string[] args)
        {
            #region Parse Parameters
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-add":
                        if (job != jobs.unknown)
                        {
                            Console.WriteLine("Can't do more than one job. Run programm twice...");
                            return;
                        }
                        job = jobs.add;
                        break;
                    case "-clean":
                        if (job != jobs.unknown)
                        {
                            Console.WriteLine("Can't do more than one job. Run programm twice...");
                            return;
                        }
                        job = jobs.removeUnexisting;
                        break;
                    case "-truncate":
                        if (job != jobs.unknown)
                        {
                            Usage();
                            Console.WriteLine("Can't do more than one job. Run programm twice...");
                            return;
                        }
                        job = jobs.truncate;
                        break;
                    case "-s":
                        connectinString = args[++i];
                        break;
                    case "-db":
                        dbType = args[++i].ToLower();
                        break;
                    case "-provider":
                        provider = args[++i].ToLower();
                        break;
                    case "-fn":
                        if (rootPath != String.Empty)
                        {
                            Usage();
                            Console.WriteLine("Filename OR Rootdirectory...");
                            return;
                        }
                        fileName = args[++i];
                        break;
                    case "-rd":
                        if (fileName != String.Empty)
                        {
                            Usage();
                            Console.WriteLine("Filename OR Rootdirectory...");
                            return;
                        }
                        rootPath = args[++i];
                        break;
                    case "-f":
                        Filters = args[++i];
                        break;
                }

            }
            #endregion

            #region Check Parameters
            if (connectinString == String.Empty)
            {
                Usage();
                Console.WriteLine("No connection string...");
                return;
            }
            switch (job)
            {
                case jobs.removeUnexisting:
                case jobs.truncate:
                    break;
                case jobs.add:
                    if (fileName == String.Empty &&
                        (rootPath == String.Empty || Filters == String.Empty))
                    {
                        Usage();
                        Console.WriteLine("No file or rootdirectory and filter defined...");
                        return;
                    }
                    break;
                case jobs.unknown:
                    Usage();
                    Console.WriteLine("No job defined...");
                    return;
            }
            #endregion

            DateTime dt = DateTime.Now;

            string mdb = ConfigTextStream.ExtractValue(connectinString, "mdb");
            string dsname = ConfigTextStream.ExtractValue(connectinString, "dsname");
            string connStr = ConfigTextStream.RemoveValue(connectinString, "dsname");

            IFeatureDataset ds = null;
            if (mdb != String.Empty)
            {
                AccessFDB fdb = new AccessFDB();
                fdb.Open(connStr);
                IFeatureDataset dataset = fdb[dsname];
                if (dataset == null)
                {
                    Console.WriteLine("Error opening dataset: " + fdb.lastErrorMsg);
                    return;
                }
                //dataset.ConnectionString = connectinString;
                if (!dataset.Open())
                {
                    Console.WriteLine("Error opening dataset: " + dataset.lastErrorMsg);
                    return;
                }
                ds = dataset;
            }
            else if (dbType == "sql")
            {
                SqlFDB fdb = new SqlFDB();
                fdb.Open(connStr);
                IFeatureDataset dataset = fdb[dsname];
                if (dataset == null)
                {
                    Console.WriteLine("Error opening dataset: " + fdb.lastErrorMsg);
                    return;
                }
                //dataset.ConnectionString = connectinString;
                if (!dataset.Open())
                {
                    Console.WriteLine("Error opening dataset: " + dataset.lastErrorMsg);
                    return;
                }
                ds = dataset;
            }
            else if (dbType == "postgres")
            {
                pgFDB fdb = new pgFDB();
                fdb.Open(connStr);
                IFeatureDataset dataset = fdb[dsname];
                if (dataset == null)
                {
                    Console.WriteLine("Error opening dataset: " + fdb.lastErrorMsg);
                    return;
                }
                //dataset.ConnectionString = connectinString;
                if (!dataset.Open())
                {
                    Console.WriteLine("Error opening dataset: " + dataset.lastErrorMsg);
                    return;
                }
                ds = dataset;
            }
            else if (dbType == "sqlite")
            {
                SQLiteFDB fdb = new SQLiteFDB();
                fdb.Open(connStr);
                IFeatureDataset dataset = fdb[dsname];
                if (dataset == null)
                {
                    Console.WriteLine("Error opening dataset: " + fdb.lastErrorMsg);
                    return;
                }
                //dataset.ConnectionString = connectinString;
                if (!dataset.Open())
                {
                    Console.WriteLine("Error opening dataset: " + dataset.lastErrorMsg);
                    return;
                }
                ds = dataset;
            }

            IRasterFileDataset rds = null;
            if (provider == "gdal")
            {
                rds = PlugInManager.Create(new Guid("43DFABF1-3D19-438c-84DA-F8BA0B266592")) as IRasterFileDataset;
            }
            else if (provider == "raster")
            {
                rds = PlugInManager.Create(new Guid("D4812641-3F53-48eb-A66C-FC0203980C79")) as IRasterFileDataset;
            }

            Dictionary<string, Guid> providers = new Dictionary<string, Guid>();
            if (rds != null)
            {
                foreach (string format in rds.SupportedFileFilter.Split('|'))
                {
                    string extension = format;

                    int pos = format.LastIndexOf(".");
                    if (pos > 0)
                        extension = format.Substring(pos, format.Length - pos);

                    providers.Add(extension, PlugInManager.PlugInID(rds));
                    Console.WriteLine("Provider " + extension + ": " + rds.ToString() + " {" + PlugInManager.PlugInID(rds).ToString() + "}");
                }
            }
            if (providers.Count == 0)
                providers = null;

            switch (job)
            {
                case jobs.truncate:
                    Truncate(ds, dsname + "_IMAGE_POLYGONS");
                    break;
                case jobs.removeUnexisting:
                    RemoveUnexisting(ds);
                    CalculateExtent(ds);
                    break;
                case jobs.add:
                    if (fileName != String.Empty)
                    {
                        if (!ImportFiles(ds, fileName.Split(';'), providers))
                        {
                            if (!continueOnError)
                                return;
                        }
                    }
                    else if (rootPath != String.Empty && Filters != String.Empty)
                    {
                        if (!ImportDirectory(ds, new DirectoryInfo(rootPath), Filters.Split(';'), providers))
                        {
                            if (!continueOnError)
                                return;
                        }
                    }
                    CalculateExtent(ds);
                    break;
            }
            Console.WriteLine("\n" + ((TimeSpan)(DateTime.Now - dt)).TotalSeconds + "s");
            Console.WriteLine("done...");
        }

        static void Truncate(IFeatureDataset ds, string fcname)
        {
            if (!((AccessFDB)ds.Database).TruncateTable(fcname))
            {
                Console.WriteLine("Error: " + ds.lastErrorMsg);
            }

            AccessFDB fdb = ds.Database as AccessFDB;
            if (fdb != null)
            {
                IFeatureClass fc = fdb.GetFeatureclass(ds.DatasetName, ds.DatasetName + "_IMAGE_POLYGONS");
                fdb.CalculateExtent(fc);
                //fdb.RebuildSpatialIndex(ds.DatasetName + "_IMAGE_POLYGONS");
            }
        }

        static bool RemoveUnexisting(IFeatureDataset ds) 
        {
            FDBImageDataset import = new FDBImageDataset(ds.Database as IImageDB, ds.DatasetName);
            return import.RemoveUnexisting();
        }

        static bool ImportFiles(IFeatureDataset ds, string [] filenames, Dictionary<string,Guid> providers)
        {
            FDBImageDataset import = new FDBImageDataset(ds.Database as IImageDB, ds.DatasetName);
            import.handleNonGeorefAsError = !(rootPath != String.Empty);

            foreach (string filename in filenames)
            {
                //Console.WriteLine("Import: " + filename);
                if (!import.Import(filename, providers))
                {
                    Console.WriteLine("Error: " + import.lastErrorMessage);
                    if (!continueOnError)
                        return false;
                }
            }
            return true;
        }

        static bool ImportDirectory(IFeatureDataset ds, DirectoryInfo di, string[] filters, Dictionary<string,Guid> providers)
        {
            foreach (string filter in filters)
            {
                WildcardEx wildcard = new WildcardEx(filter, RegexOptions.IgnoreCase);

                FileInfo[] fis = di.GetFiles(filter);

                List<string> filenames = new List<string>();
                foreach (FileInfo fi in fis)
                {
                    if (wildcard.IsMatch(fi.Name))
                    {
                        filenames.Add(fi.FullName);
                    }
                }

                if (filenames.Count != 0)
                {
                    if (!ImportFiles(ds, filenames.ToArray(), providers))
                    {
                        if (!continueOnError)
                            return false;
                    }
                }
            }

            foreach (DirectoryInfo sub in di.GetDirectories())
            {
                if (!ImportDirectory(ds, sub, filters, providers))
                {
                    if (!continueOnError)
                        return false;
                }
            }
            return true;
        }

        static bool CalculateExtent(IFeatureDataset ds)
        {
            Console.WriteLine("\nCalculete new Extent"); 
            AccessFDB fdb = ds.Database as AccessFDB;
            if (fdb != null)
            {
                IFeatureClass fc = fdb.GetFeatureclass(ds.DatasetName, ds.DatasetName + "_IMAGE_POLYGONS");
                fdb.CalculateExtent(fc);
                //fdb.ShrinkSpatialIndex(ds.DatasetName + "_IMAGE_POLYGONS");
                //fdb.RebuildSpatialIndex(ds.DatasetName + "_IMAGE_POLYGONS");
            }
            return true;
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
            Replace("\\*", ".*").
            Replace("\\?", ".") + "$";
        }

        static void Usage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("gView.Cmd.Rds.Util -db <sql|access|postgres|sqlite> -s <Source Dataset Connection String>");
            Console.WriteLine("            -add ... append files");
            Console.WriteLine("                    -fn <filename> or");
            Console.WriteLine("                    -rd <rootdirectory> -f <filter>");
            Console.WriteLine("                    -provider <first|gdal|raster>");
            Console.WriteLine("            -clean ... Remove Unexisting");
            Console.WriteLine("            -truncate ... remove all files"); 
        }
    }
}
