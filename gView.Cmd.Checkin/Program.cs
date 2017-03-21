using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.Data;
using gView.Framework.Offline;

namespace checkin
{
    class Program
    {
        static int _conflicts = 0, _inserted = 0, _updated = 0, _deleted = 0, _ignored = 0;
        static string _checkInResults = String.Empty;
        static bool _silent = false;

        static void Main(string[] args)
        {
            string source_connstr = "", source_fc = "";
            string dest_connstr = "", dest_fc = "";
            Guid source_guid = Guid.Empty, dest_guid = Guid.Empty;
            bool reconcile = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-source_connstr")
                    source_connstr = args[++i];

                else if (args[i] == "-source_guid")
                    source_guid = new Guid(args[++i]);

                else if (args[i] == "-source_fc")
                    source_fc = args[++i];

                else if (args[i] == "-dest_connstr")
                    dest_connstr = args[++i];

                else if (args[i] == "-dest_guid")
                    dest_guid = new Guid(args[++i]);

                else if (args[i] == "-dest_fc")
                    dest_fc = args[++i];

                else if (args[i] == "-reconcile")
                    reconcile = true;

                else if (args[i] == "-silent")
                    _silent = true;
            }

            if (source_connstr == "" || source_fc == "" || source_guid == Guid.Empty ||
                dest_connstr == "" || dest_fc == "" || dest_guid == Guid.Empty)
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.Checkin -source_connstr <Source Dataset Connection String>");
                Console.WriteLine("                  -source_guid <GUID of Dataset Extension>");
                Console.WriteLine("                  -source_fc <Featureclass name>");
                Console.WriteLine("                  -dest_connstr <Destination Dataset Connection String>");
                Console.WriteLine("                  -dest_guid <GUID of Dataset Extension>");
                Console.WriteLine("                  -dest_fc <Featureclass name>");
                Console.WriteLine("                  [-reconcile]");
                Console.WriteLine("                  [-silent]");
                return;
            }

            Console.WriteLine("\n" + source_fc + ":");

            PlugInManager compMan = new PlugInManager();
            IFeatureDataset sourceDS = compMan.CreateInstance(source_guid) as IFeatureDataset;
            if (sourceDS==null)
            {
                Console.WriteLine("ERROR: Component with GUID '" + source_guid.ToString() + "' not found...");
                return;
            }
            IFeatureDataset destDS = compMan.CreateInstance(dest_guid) as IFeatureDataset;
            if (destDS == null)
            {
                Console.WriteLine("ERROR: Component with GUID '" + dest_guid.ToString() + "' not found...");
                return;
            }

            sourceDS.ConnectionString = source_connstr;
            destDS.ConnectionString = dest_connstr;
            if (!sourceDS.Open() || !(sourceDS.Database is IFeatureDatabaseReplication))
            {
                Console.WriteLine("ERROR: Component with GUID '" + source_guid.ToString() + "' is not a replicatable feature dataset...");
                return;
            }
            if (!destDS.Open() || !(destDS.Database is IFeatureDatabaseReplication))
            {
                Console.WriteLine("ERROR: Component with GUID '" + dest_guid.ToString() + "' is not a replicatable feature dataset...");
                return;
            }

            IDatasetElement sourceElement = sourceDS[source_fc];
            IDatasetElement destElement = destDS[dest_fc];
            IFeatureClass sourceFC = ((sourceElement != null) ? sourceElement.Class as IFeatureClass : null);
            IFeatureClass destFC = ((destElement != null) ? destElement.Class as IFeatureClass : null);

            if (sourceFC == null)
            {
                Console.WriteLine("ERROR: Featureclass " + source_fc + " is not available...");  
                return;
            }
            if (destFC == null)
            {
                Console.WriteLine("ERROR: Featureclass " + dest_fc + " is not available...");
                return;
            }

            string errMsg = String.Empty;
            Replication repl = new Replication();
            repl.CheckIn_ConflictDetected += new Replication.CheckIn_ConflictDetectedEventHandler(repl_CheckIn_ConflictDetected);
            repl.CheckIn_FeatureDeleted += new Replication.CheckIn_FeatureDeletedEventHandler(repl_CheckIn_FeatureDeleted);
            repl.CheckIn_FeatureInserted += new Replication.CheckIn_FeatureInsertedEventHandler(repl_CheckIn_FeatureInserted);
            repl.CheckIn_FeatureUpdated += new Replication.CheckIn_FeatureUpdatedEventHandler(repl_CheckIn_FeatureUpdated);
            repl.CheckIn_IgnoredSqlStatement += new Replication.CheckIn_IgnoredSqlStatementEventHandler(repl_CheckIn_IgnoredSqlStatement);
            repl.CheckIn_BeginPost += new Replication.CheckIn_BeginPostEventHandler(repl_CheckIn_BeginPost);
            repl.CheckIn_BeginCheckIn += new Replication.CheckIn_BeginCheckInEventHandler(repl_CheckIn_BeginCheckIn);
            repl.CheckIn_ChangeSessionLockState += new Replication.CheckIn_ChangeSessionLockStateEventHandler(repl_CheckIn_ChangeSessionLockState);
            repl.CheckIn_Message += new Replication.CheckIn_MessageEventHandler(repl_CheckIn_Message);
            
            Replication.ProcessType type = (reconcile) ?
                Replication.ProcessType.Reconcile :
                Replication.ProcessType.CheckinAndRelease;

            if (!repl.Process(destFC, sourceFC, type, out errMsg))
            {
                Console.WriteLine();
                Console.WriteLine("ERROR :" + errMsg);
                Console.WriteLine();
            }
            else
            {
                if (!String.IsNullOrEmpty(_checkInResults))
                {
                    if (_silent)
                    {
                        Console.WriteLine(_checkInResults);
                    }
                    else
                    {
                        Console.WriteLine("\n" + _checkInResults);
                    }
                }

                if (reconcile)
                {
                    Console.WriteLine("Reconcile:\t" + _inserted + "\t" + _updated + "\t" + _deleted + "\t" + _conflicts);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("\t\tINSERT\tUPDATE\tDELETE\tCONFLICTS");
                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine("Checkin  :\t" + _inserted + "\t" + _updated + "\t" + _deleted + "\t" + _conflicts);
                }
            }
            Console.WriteLine("------------------------------------------------------------");
                    
        }

        static void repl_CheckIn_Message(Replication sender, string message)
        {
            Console.WriteLine(message);
        }

        static void repl_CheckIn_ChangeSessionLockState(Replication sender, string className, Guid session_guid, Replication.LockState lockState)
        {
            if (_silent)
            {
                //Console.WriteLine("SessionLock:" + lockState.ToString());
            }
            else
            {
                Console.WriteLine("SessionLock: Class=" + className);
                Console.WriteLine("             SessionGuid=" + session_guid.ToString());
                Console.WriteLine("             LockState=" + lockState.ToString());
                Console.WriteLine();
            }
        }

        static void repl_CheckIn_BeginPost(Replication sender)
        {
            if (!_silent)
            {
                Console.WriteLine();
                Console.WriteLine("Reconcile:");
            }
            _checkInResults = "\nCheckin results:";
            _checkInResults = "\n\t\tINSERT\tUPDATE\tDELETE\tCONFLICTS";
            _checkInResults += "\n------------------------------------------------------------";
            _checkInResults += "\nCheckin  :\t" + _inserted + "\t" + _updated + "\t" + _deleted + "\t" + _conflicts;
            //_checkInResults += "\n" + _inserted + " feature(s) inserted";
            //_checkInResults += "\n" + _updated + " feature(s) updated";
            //_checkInResults += "\n" + _deleted + " feature(s) deleted";
            //_checkInResults += "\n" + _conflicts + " conflict(s) detected";
            //_checkInResults += "\n------------------------------------------------------------";

            _inserted = _updated = _deleted = _conflicts = 0;
        }

        static void repl_CheckIn_BeginCheckIn(Replication sender)
        {
            if (!_silent)
            {
                Console.WriteLine();
                Console.WriteLine("Checkin:");
            }
        }

        static void repl_CheckIn_IgnoredSqlStatement(Replication sender, int count_ignored, Replication.SqlStatement statement)
        {
            switch (statement)
            {
                case Replication.SqlStatement.INSERT:
                    if(!_silent) Console.Write("...(i)");
                    break;
                case Replication.SqlStatement.UPDATE:
                    if(!_silent) Console.Write("...(u)");
                    break;
                case Replication.SqlStatement.DELETE:
                    if(!_silent) Console.Write("...(d)");
                    break;
            }
            _ignored++;
        }

        static void repl_CheckIn_FeatureUpdated(Replication sender, int count_updated)
        {
            if (!_silent) Console.Write("...u");
            _updated++;
        }

        static void repl_CheckIn_FeatureInserted(Replication sender, int count_inserted)
        {
            if (!_silent) Console.Write("...i");
            _inserted++;
        }

        static void repl_CheckIn_FeatureDeleted(Replication sender, int count_deleted)
        {
            if (!_silent) Console.Write("...d");
            _deleted++;
        }

        static void repl_CheckIn_ConflictDetected(Replication sender, int count_confilicts)
        {
            if (!_silent) Console.Write("...c");
            _conflicts++;
        }
    }
}
