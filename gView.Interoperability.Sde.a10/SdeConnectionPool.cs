using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using gView.SDEWrapper.a10;
using gView.Framework.system;
using gView.Framework.IO;
using gView.Framework.Data;

namespace gView.Interoperability.Sde.a10
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
            CloseAllConnections(null);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public string LastErrorMessage { get { return _errMsg;  } }

        private bool OpenConnection(SdeConnection sdeConnection, IDataset dataset)
        {
            _errMsg = "";

            if (sdeConnection.SeConnection.handle != IntPtr.Zero) CloseConnection(sdeConnection);
            
            string server = ConfigTextStream.ExtractValue(_connectionString, "server");
            string instance = ConfigTextStream.ExtractValue(_connectionString, "instance");
            string database = ConfigTextStream.ExtractValue(_connectionString, "database");
            string username = ConfigTextStream.ExtractValue(_connectionString, "usr");
            string password = ConfigTextStream.ExtractValue(_connectionString, "pwd");
            string pooling = ConfigTextStream.ExtractValue(_connectionString, "pooling");

            if (!String.IsNullOrWhiteSpace(pooling) && 
                (pooling.ToLower() == "false" || pooling.ToLower() == "no"))
                sdeConnection.Pooling = false;
            else
                sdeConnection.Pooling = true;

            sdeConnection.Dataset = dataset;

            SE_ERROR error = new SE_ERROR();
            try
            {
                System.Int32 errCode=Wrapper10.SE_connection_create(
                    server,
                    instance,
                    database,
                    username,
                    password,
                    ref error,
                    ref sdeConnection.SeConnection);
                if (errCode != 0)
                {
                    error.sde_error = errCode;
                    _errMsg = errCode + " " + Wrapper10.GetErrorMsg(sdeConnection.SeConnection, error);
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
            //sdeConnection.FreeStream();
            if (sdeConnection.SeConnection.handle != IntPtr.Zero)
            {
                Wrapper10.SE_connection_free(sdeConnection.SeConnection);
                sdeConnection.SeConnection.handle = IntPtr.Zero;
            }
            sdeConnection.Dataset = null;
        }

        public SdeConnection Alloc(IDataset dataset)
        {
            lock (_thisLock)
            {
                // Freie Connection suchen...
                foreach (SdeConnection seConnection in ListOperations<SdeConnection>.Clone(_freeConnections))
                {
                    if (seConnection.SeConnection.handle != IntPtr.Zero)
                    {
                        _freeConnections.Remove(seConnection);
                        _usedConnections.Add(seConnection);
                        seConnection.Dataset = dataset;
                        return seConnection;
                    }
                }

                // Freie Connection suchen, die bereits geschlossen wurde...
                foreach (SdeConnection SeConnection in ListOperations<SdeConnection>.Clone(_freeConnections))
                {
                    if (SeConnection.SeConnection.handle == IntPtr.Zero)
                    {
                        if (OpenConnection(SeConnection, dataset))
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
                    SdeConnection seConnection = new SdeConnection(dataset);
                    if (OpenConnection(seConnection, dataset))
                    {
                        _usedConnections.Add(seConnection);
                        return seConnection;
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
                    if (sdeConnection.Pooling == true)
                    {
                        _freeConnections.Add(sdeConnection);
                        sdeConnection.LastUse = DateTime.Now;
                    }
                    else
                    {
                        if (sdeConnection.SeConnection.handle != IntPtr.Zero)
                        {
                            CloseConnection(sdeConnection);
                        }
                    }
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
                    if (sdeConnection.SeConnection.handle != IntPtr.Zero && ts.TotalSeconds > totalSeconds)
                    {
                        CloseConnection(sdeConnection);
                    }
                }
            }
        }

        public void CloseAllConnections(IDataset dataset)
        {
            lock (_thisLock)
            {
                foreach (SdeConnection sdeConnection in _freeConnections)
                {
                    if (dataset == null || sdeConnection.Dataset == dataset)
                    {
                        if (sdeConnection.SeConnection.handle != IntPtr.Zero)
                        {
                            CloseConnection(sdeConnection);
                        }
                    }
                }
                foreach (SdeConnection sdeConnection in ListOperations<SdeConnection>.Clone(_usedConnections))
                {
                    if (dataset == null || sdeConnection.Dataset == dataset)
                    {
                        CloseConnection(sdeConnection);
                        _usedConnections.Remove(sdeConnection);
                        _freeConnections.Add(sdeConnection);
                    }
                }
            }
        }

        #region IDisposable Member

        public void Dispose()
        {
            CloseAllConnections(null);
            _freeConnections.Clear();
            _usedConnections.Clear();

            GC.SuppressFinalize(this); // Finalize kann entfallen...
        }

        public void Dispose(IDataset dataset)
        {
            if (dataset == null)
                this.Dispose();
            else
                CloseAllConnections(dataset);
        }

        #endregion
    }
}
