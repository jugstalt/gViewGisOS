using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.SDEWrapper.x64;
using gView.Framework.system;
using gView.Framework.IO;
using System.IO;

namespace gView.Interoperability.Sde.x64
{
    [gView.Framework.system.RegisterPlugIn("CE42218B-6962-48c9-9BAC-39E4C5003AE5")]
    public class SdeDataset : DatasetMetadata, IFeatureDataset, IDisposable, IDataset2, IPersistable, IPlugInDependencies
    {
        private string _connStr = "",_errMsg="";
        //private SmartSdeConnection _sConnection;
        //private SdeConnection _current = null;
        private List<IDatasetElement> _elements = new List<IDatasetElement>();
        private DatasetState _state = DatasetState.unknown;

        public SdeDataset()
        {
        }

        ~SdeDataset()
        {
        }

        /*
        internal SdeConnection AllocConnection()
        {
            if (_sConnection == null) return null;
            return _current = _sConnection.AllocConnection();
        }

        internal void FreeConnection()
        {
            if (_sConnection == null) return;
            _current = null;
            _sConnection.FreeConnection();
        }
        */

        internal SE_COORDREF_64 GetSeCoordRef(ArcSdeConnection connection, string table, string spatialColumnName, ref SE_ENVELOPE envelope)
        {
            _errMsg = "";
            if (connection == null || connection.SeConnection.handle == 0)
            {
                _errMsg = "GetSeCoordRef:\n No Connection allocated!";
                return new SE_COORDREF_64();
            }

            SE_COORDREF_64 coordRef = new SE_COORDREF_64();
            SE_LAYERINFO_64 layerInfo = new SE_LAYERINFO_64();
            Int64 _err_no = 0;

            try
            {
                _err_no = Wrapper92_64.SE_coordref_create(ref coordRef);
                if (_err_no != 0) return new SE_COORDREF_64();

                _err_no = Wrapper92_64.SE_layerinfo_create(coordRef, ref layerInfo);
                if (_err_no != 0) return new SE_COORDREF_64();

                _err_no = Wrapper92_64.SE_layer_get_info(connection.SeConnection, table, spatialColumnName, layerInfo);
                if (_err_no != 0) return new SE_COORDREF_64();

                _err_no = Wrapper92_64.SE_layerinfo_get_coordref(layerInfo, coordRef);
                if (_err_no != 0) return new SE_COORDREF_64();

                _err_no = Wrapper92_64.SE_coordref_get_xy_envelope(coordRef, ref envelope);
                if (_err_no != 0) return new SE_COORDREF_64();

                return coordRef;
            }
            catch (Exception ex)
            {
                _errMsg = "GetSeCoordRef:\n " + ex.Message + "\n" + ex.StackTrace;
                return new SE_COORDREF_64();
            }
            finally
            {
                if (layerInfo.handle != 0) Wrapper92_64.SE_layerinfo_free(layerInfo);
    
                if (_err_no != 0)
                {
                    if (coordRef.handle != 0) Wrapper92_64.SE_coordref_free(coordRef);
                    _errMsg = Wrapper92_64.GetErrorMsg(new SE_CONNECTION_64(), _err_no);
                }
            }
        }
        internal void FreeSeCoordRef(SE_COORDREF_64 coordRef)
        {
            if (coordRef.handle != 0)
                Wrapper92_64.SE_coordref_free(coordRef);
            coordRef.handle = 0;
        }

        #region IFeatureDataset Member

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get 
            {
                Envelope env = null;
                foreach (IDatasetElement element in _elements)
                {
                    if (element.Class is IFeatureClass && ((IFeatureClass)element.Class).Envelope != null)
                    {
                        if (env == null)
                        {
                            env = new Envelope(((IFeatureClass)element.Class).Envelope);
                        }
                        else
                        {
                            env.Union(((IFeatureClass)element.Class).Envelope);
                        }
                    }
                }

                return env == null ? new Envelope() : env;
            }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

        #endregion

        #region IDataset Member

        public void Dispose()
        {
            ArcSdeConnection.DisposePool(_connStr);
            _elements.Clear();
            GC.SuppressFinalize(this);
        }

        public string ConnectionString
        {
            get
            {
                return _connStr;
            }
            set
            {
                _connStr = value;
                if (_connStr.ToLower().StartsWith("sde:"))
                {
                    _connStr = _connStr.Substring(4, _connStr.Length - 4);
                }
            }
        }

        public string DatasetGroupName
        {
            get { return "ESRI Sde"; }
        }

        public string DatasetName
        {
            get { return "ESRI Spatial Database Engine"; }
        }

        public string ProviderName
        {
            get { return "ESRI"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public bool Open()
        {
            if (_connStr == "") return false;

            //if (_sConnection != null)
            //{
            //    _sConnection.Dispose();
            //}
            //_sConnection = new SmartSdeConnection(_connStr);

            _state = DatasetState.opened;
            return true;
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public int order
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public IDatasetEnum DatasetEnum
        {
            get { return null; }
        }

        public List<IDatasetElement> Elements
        {
            get
            {
                if (_elements.Count == 0)
                {
                    IntPtr ptr = new IntPtr(0);
                    System.Int64 num = 0;

                    //SdeConnection conn = _sConnection.AllocConnection();
                    //if (conn == null) return _elements;
                    using (ArcSdeConnection conn = new ArcSdeConnection(_connStr))
                    {
                        if (!conn.Open()) return _elements;

                        System.Int64 status = Wrapper92_64.SE_layer_get_info_list(conn.SeConnection, ref ptr, ref num);
                        if (status != 0)
                        {
                            return _elements;
                        }

                        unsafe
                        {
                            IntPtr* layerInfo = (System.IntPtr*)ptr;

                            for (int i = 0; i < num; i++)
                            {
                                try
                                {
                                    SE_LAYERINFO_64 seLayerInfo = new SE_LAYERINFO_64();
                                    seLayerInfo.handle = (Int32)layerInfo[i];
                                    SdeFeatureClass fc = new SdeFeatureClass(this, conn.SeConnection, seLayerInfo);
                                    if (fc.Name != "")
                                    {
                                        _elements.Add(new DatasetElement(fc));
                                    }
                                }
                                catch { }
                            }
                        }
                        conn.Close();
                    }

                    if (num > 0) Wrapper92_64.SE_layer_free_info_list(num, ptr);
                }

                return _elements;
            }
        }

        public string Query_FieldPrefix
        {
            get { return ""; }
        }

        public string Query_FieldPostfix
        {
            get { return ""; }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get { return null; }
        }

        public IDatasetElement this[string title]
        {
            get
            {
                SE_LAYERINFO_64 layerInfo = new SE_LAYERINFO_64();
                try
                {
                    SdeFeatureClass fc = null;
                    //SdeConnection connection = _sConnection.AllocConnection();
                    using (ArcSdeConnection connection = new ArcSdeConnection(_connStr))
                    {
                        if (connection.Open())
                        {
                            if (Wrapper92_64.SE_layerinfo_create(new SE_COORDREF_64(), ref layerInfo) != 0) return null;
                            if (Wrapper92_64.SE_layer_get_info(connection.SeConnection, title, "", layerInfo) != 0) return null;
                            fc = new SdeFeatureClass(this, connection.SeConnection, layerInfo);
                        }
                        connection.Close();
                    }
                    //_sConnection.FreeConnection();
                    return  new DatasetElement(fc);
                }
                finally
                {
                    if (layerInfo.handle != 0) Wrapper92_64.SE_layerinfo_free(layerInfo);
                }
            }
        }

        public void RefreshClasses()
        {
        }
        #endregion

        #region IDataset2 Member

        public IDataset2 EmptyCopy()
        {
            SdeDataset dataset = new SdeDataset();
            dataset.ConnectionString = this.ConnectionString;
            dataset.Open();

            return dataset;
        }

        public void AppendElement(string elementName)
        {
            if (_elements == null) _elements = new List<IDatasetElement>();

            foreach (IDatasetElement e in _elements)
            {
                if (e.Title == elementName) return;
            }

            IDatasetElement element = this[elementName];
            if (element != null) _elements.Add(element);
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            string connectionString = (string)stream.Load("connectionstring", "");

            this.ConnectionString = connectionString;
            this.Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("connectionstring", _connStr);
        }

        #endregion

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return SdeDataset.hasUnsolvedDependencies;
        }

        #endregion

        public static bool hasUnsolvedDependencies
        {
            get
            {
                try
                {
                    if (!gView.Framework.system.Wow.Is64BitOperatingSystem)
                        return true;

                    //FileInfo fi = new FileInfo("sde83.dll");
                    //if (!fi.Exists) return true;
                    //fi = new FileInfo("pe83.dll");
                    //if (!fi.Exists) return true;
                    //fi = new FileInfo("sg83.dll");
                    //if (!fi.Exists) return true;

                    FileInfo fi = new FileInfo("sde.dll");
                    if (!fi.Exists) return true;
                    fi = new FileInfo("pe.dll");
                    if (!fi.Exists) return true;
                    fi = new FileInfo("sg.dll");
                    if (!fi.Exists) return true;

                    return false;
                }
                catch (Exception ex)
                {
                    return true;
                }
            }
        }
    }
}
