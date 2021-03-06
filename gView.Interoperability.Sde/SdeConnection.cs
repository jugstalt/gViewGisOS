using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using gView.SDEWrapper;
using gView.Framework.system;
using gView.Framework.IO;
using gView.Framework.Data;

namespace gView.Interoperability.Sde
{
    public class ArcSdeConnection : IDisposable
    {
        private string _errMsg="",_connectionString="";
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

        public SE_CONNECTION SeConnection
        {
            get
            {
                if (_sdeConnection == null) return new SE_CONNECTION();
                return _sdeConnection.SeConnection;
            }
        }

        public SE_STREAM SeStream
        {
            get
            {
                if (_sdeConnection == null) return new SE_STREAM();
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
            _errMsg = "";
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
        public SE_CONNECTION SeConnection = new SE_CONNECTION();
        private SE_STREAM _stream = new SE_STREAM();

        public SE_STREAM SeStream
        {
            get
            {
                if (_stream.handle == 0 && SeConnection.handle != 0)
                {
                    Wrapper92.SE_stream_create(SeConnection, ref _stream);
                }
                return _stream;
            }
        }
        internal void FreeStream()
        {
            if (_stream.handle != 0)
            {
                Wrapper92.SE_stream_close(_stream, true);
                Wrapper92.SE_stream_free(_stream);
                _stream.handle = 0;
            }
        }
        public void ResetStream()
        {
            if (_stream.handle == 0) return;

            Wrapper92.SE_stream_close(_stream, true);
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

        protected override SdeConnection OpenConnection(IDataset dataset)
        {
            _errMsg = "";

            SdeConnection connection = new SdeConnection();

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
                    ref connection.SeConnection) != 0)
                {
                    _errMsg = Wrapper92.GetErrorMsg(connection.SeConnection, error);
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
                Wrapper92.SE_connection_free(connection.SeConnection);
                connection.SeConnection.handle = 0;
            }
        }
    }
}
