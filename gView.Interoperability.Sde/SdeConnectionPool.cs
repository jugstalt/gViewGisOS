using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using gView.SDEWrapper;
using gView.Framework.system;
using gView.Framework.IO;

namespace gView.Interoperability.Sde
{
    internal class SdeConnectionPool : IDisposable
    {
        private List<SdeConnection> _freeConnections, _usedConnections;
        private int _maxConnections = 1;
        private string _connectionString, _errMsg;
        private object _thisLock = new object();

        public SdeConnectionPool(int maxConnections,string connectionString)
        {
            _maxConnections = maxConnections;
            _connectionString = connectionString;
            _freeConnections = new List<SdeConnection>(maxConnections);
            _usedConnections = new List<SdeConnection>(maxConnections);
        }

        ~SdeConnectionPool()
        {
            CloseAllConnections();
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        private bool OpenConnection(SdeConnection SdeConnection)
        {
            if (SdeConnection.SeConnection.handle != 0) CloseConnection(SdeConnection);
            
            string server = ConfigTextStream.ExtractValue(_connectionString, "server");
            string instance = ConfigTextStream.ExtractValue(_connectionString, "instance");
            string database = ConfigTextStream.ExtractValue(_connectionString, "database");
            string username = ConfigTextStream.ExtractValue(_connectionString, "usr");
            string password = ConfigTextStream.ExtractValue(_connectionString, "pwd");

            SE_ERROR error = new SE_ERROR();
            try
            {
                if (Wrapper92.SE_connection_create(
                    server,
                    instance,
                    database,
                    username,
                    password,
                    ref error,
                    ref SdeConnection.SeConnection) != 0)
                {
                    _errMsg = Wrapper92.GetErrorMsg(SdeConnection.SeConnection, error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _errMsg = "SDE ERROR: " + ex.Message;
                return false;
            }

            return true;
        }
        private void CloseConnection(SdeConnection sdeConnection)
        {
            sdeConnection.FreeStream();
            if (sdeConnection.SeConnection.handle != 0)
            {
                Wrapper92.SE_connection_free(sdeConnection.SeConnection);
                sdeConnection.SeConnection.handle = 0;
            }
        }

        public SdeConnection Alloc()
        {
            lock (_thisLock)
            {
                // Freie Connection suchen...
                foreach (SdeConnection SeConnection in ListOperations<SdeConnection>.Clone(_freeConnections))
                {
                    if (SeConnection.SeConnection.handle != 0)
                    {
                        _freeConnections.Remove(SeConnection);
                        _usedConnections.Add(SeConnection);
                        return SeConnection;
                    }
                }

                // Freie Connection suchen, die bereits geschlossen wurde...
                foreach (SdeConnection SeConnection in ListOperations<SdeConnection>.Clone(_freeConnections))
                {
                    if (SeConnection.SeConnection.handle == 0)
                    {
                        if (OpenConnection(SeConnection))
                        {
                            _freeConnections.Remove(SeConnection);
                            _usedConnections.Add(SeConnection);
                            return SeConnection;
                        }
                    }
                }

                // Neue Connection anlegen
                if (_usedConnections.Count + _freeConnections.Count < _maxConnections)
                {
                    SdeConnection SeConnection = new SdeConnection();
                    if (OpenConnection(SeConnection))
                    {
                        _usedConnections.Add(SeConnection);
                        return SeConnection;
                    }
                }

                return null;
            }
        }

        public void Free(SdeConnection sdeConnection) 
        {
            if (sdeConnection == null) return;
            lock (_thisLock)
            {
                if (_usedConnections.IndexOf(sdeConnection) != -1)
                {
                    _usedConnections.Remove(sdeConnection);
                    _freeConnections.Add(sdeConnection);
                    sdeConnection.ResetStream();
                    sdeConnection.LastUse = DateTime.Now;
                }
            }
        }

        public void CloseUnusedConnections(int totalSeconds)
        {
            //
            // Verbinden schließen, die länger als totalseconds nicht mehr begraucht wurden
            //
            lock (_thisLock)
            {
                foreach (SdeConnection sdeConnection in _freeConnections)
                {
                    TimeSpan ts = DateTime.Now - sdeConnection.LastUse;
                    if (sdeConnection.SeConnection.handle != 0 && ts.TotalSeconds > totalSeconds)
                    {
                        CloseConnection(sdeConnection);
                    }
                }
            }
        }

        public void CloseAllConnections()
        {
            foreach (SdeConnection sdeConnection in _freeConnections)
            {
                if (sdeConnection.SeConnection.handle != 0)
                {
                    CloseConnection(sdeConnection);
                }
            }
            foreach (SdeConnection sdeConnection in ListOperations<SdeConnection>.Clone(_usedConnections))
            {
                CloseConnection(sdeConnection);
                _usedConnections.Remove(sdeConnection);
                _freeConnections.Add(sdeConnection);
            }
        }

        #region IDisposable Member

        public void Dispose()
        {
            CloseAllConnections();
            _freeConnections.Clear();
            _usedConnections.Clear();

            GC.SuppressFinalize(this); // Finalize kann entfallen...
        }

        #endregion
    }
}
