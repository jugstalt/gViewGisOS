using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using gView.SDEWrapper.x64;
using gView.Framework.system;
using gView.Framework.IO;

namespace gView.Interoperability.Sde.x64
{
    public class ArcSdeConnection : IDisposable
    {
        private string _connectionString="";
        private SdeConnectionPool _pool;
        private SdeConnection _sdeConnection = null;

        public ArcSdeConnection() { }
        public ArcSdeConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        ~ArcSdeConnection() 
        {
            this.Close();
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public SE_CONNECTION_64 SeConnection
        {
            get
            {
                if (_sdeConnection == null) return new SE_CONNECTION_64();
                return _sdeConnection.SeConnection;
            }
        }

        public SE_STREAM_64 SeStream
        {
            get
            {
                if (_sdeConnection == null) return new SE_STREAM_64();
                return _sdeConnection.SeStream;
            }
        }

        public void ResetStream()
        {
            if (_sdeConnection == null) return;

            _sdeConnection.ResetStream();
        }
        public bool Open()
        {
            _pool = Globals.ConnectionManager[_connectionString];
            if (_pool == null) return true;
            _sdeConnection = _pool.Alloc();

            return (_sdeConnection != null);
        }

        public void Close()
        {
            if (_pool == null || _sdeConnection == null) return;
            _pool.Free(_sdeConnection);
            _sdeConnection = null;
        }

        #region IDisposable Member

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this); // Finalize kann entfallen...
        }

        #endregion 

        public static void DisposeAllPools()
        {
            Globals.ConnectionManager.Dispose();
        }

        public static void DisposePool(string connectionString)
        {
            SdeConnectionPool pool = Globals.ConnectionManager[connectionString];
            if (pool == null) return;

            pool.Dispose();
        }
    }


    internal class SdeConnection : DatabaseConnection
    {
        public DateTime LastUse = DateTime.Now;
        public SE_CONNECTION_64 SeConnection = new SE_CONNECTION_64();
        private SE_STREAM_64 _stream = new SE_STREAM_64();

        public SE_STREAM_64 SeStream
        {
            get
            {
                if (_stream.handle == 0 && SeConnection.handle != 0)
                {
                    Wrapper92_64.SE_stream_create(SeConnection, ref _stream);
                }
                return _stream;
            }
        }
        internal void FreeStream()
        {
            if (_stream.handle != 0)
            {
                Wrapper92_64.SE_stream_close(_stream, true);
                Wrapper92_64.SE_stream_free(_stream);
                _stream.handle = 0;
            }
        }
        public void ResetStream()
        {
            if (_stream.handle == 0) return;

            Wrapper92_64.SE_stream_close(_stream, true);
        }
    }

    class SmartSdeConnection : SmartDatabaseConnection<SdeConnection>
    {
        private string _connectionString = "", _errMsg = "";
        
        public SmartSdeConnection(string connectionString) : base()
        {
            _connectionString = connectionString;
        }

        public string LastErrorMsg
        {
            get { return _errMsg; }
        }

        protected override SdeConnection OpenConnection()
        {
            _errMsg = "";

            SdeConnection connection = new SdeConnection();

            string server = ConfigTextStream.ExtractValue(_connectionString, "server");
            string instance = ConfigTextStream.ExtractValue(_connectionString, "instance");
            string database = ConfigTextStream.ExtractValue(_connectionString, "database");
            string username = ConfigTextStream.ExtractValue(_connectionString, "usr");
            string password = ConfigTextStream.ExtractValue(_connectionString, "pwd");

            SE_ERROR_64 error = new SE_ERROR_64();
            try
            {
                if (Wrapper92_64.SE_connection_create(
                    server,
                    instance,
                    database,
                    username,
                    password,
                    ref error,
                    ref connection.SeConnection) != 0)
                {
                    _errMsg = Wrapper92_64.GetErrorMsg(connection.SeConnection, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _errMsg = "SDE ERROR: " + ex.Message;
                return null;
            }
            return connection;
        }

        protected override void CloseConnection(SdeConnection connection)
        {
            if (connection == null) return;

            if (connection.SeConnection.handle != 0)
            {
                Wrapper92_64.SE_connection_free(connection.SeConnection);
                connection.SeConnection.handle = 0;
            }
        }
    }
}
