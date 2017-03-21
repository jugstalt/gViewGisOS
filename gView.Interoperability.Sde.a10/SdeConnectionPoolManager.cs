using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace gView.Interoperability.Sde.a10
{
    internal class Globals
    {

        private static SdeConnectionPoolManager _connectionManager = null;

        public static SdeConnectionPoolManager ConnectionManager
        {
            get
            {
                if (_connectionManager != null) return _connectionManager;

                _connectionManager = new SdeConnectionPoolManager(0,200);
                return _connectionManager;
            }
        }
    }

    internal class SdeConnectionPoolManager : IDisposable
    {
        private List<SdeConnectionPool> _pools;
        private int _maxConnectionsPerPool, _maxPools;
        private object _lockThis = new object();
        private BackgroundWorker _tasker;

        public SdeConnectionPoolManager(int maxPools, int maxConnectionsPerPool)
        {
            _maxConnectionsPerPool = maxConnectionsPerPool;
            _maxPools = maxPools;

            _pools = new List<SdeConnectionPool>(_maxPools);

            if (!gView.Framework.system.SystemVariables.IsWebHosted)
            {
                _tasker = new BackgroundWorker();
                _tasker.DoWork += new DoWorkEventHandler(_tasker_DoWork);
                _tasker.RunWorkerAsync();
            }
        }

        void _tasker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(5000);
                foreach (SdeConnectionPool pool in _pools)
                {
                    pool.CloseUnusedConnections(300);
                }
            }
        }

        ~SdeConnectionPoolManager()
        {
            FreeIt();
        }

        public SdeConnectionPool this[string connectionString]
        {
            get
            {
                lock (_lockThis)
                {
                    foreach (SdeConnectionPool pool in _pools)
                    {
                        if (pool.ConnectionString == connectionString) return pool;
                    }
                    if (_maxPools > 0 && _pools.Count >= _maxPools) return null;

                    SdeConnectionPool newpool = new SdeConnectionPool(_maxConnectionsPerPool, connectionString);
                    _pools.Add(newpool);
                    return newpool;
                }
            }
        }

        private void FreeIt()
        {
            foreach (SdeConnectionPool pool in _pools)
            {
                pool.Dispose();
            }
            _pools.Clear();
        } 

        #region IDisposable Member

        public void Dispose()
        {
            FreeIt();

            GC.SuppressFinalize(this); // Finalize kann entfallen...
        }

        #endregion
    }
}
