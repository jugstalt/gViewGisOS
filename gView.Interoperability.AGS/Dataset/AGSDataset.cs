using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.Geometry;
using gView.Interoperability.AGS.Proxy;
using System.Net;
using gView.Framework.Web;

namespace gView.Interoperability.AGS.Dataset
{
    [gView.Framework.system.RegisterPlugIn("2AA93053-359F-469b-AC98-25F429A3BA33")]
    public class AGSDataset : DatasetMetadata, IFeatureDataset, IRequestDependentDataset, IPersistable
    {
        internal string _connection = String.Empty;
        internal string _name = String.Empty;
        internal List<IWebServiceTheme> _themes = new List<IWebServiceTheme>();

        private IClass _class = null;
        private IEnvelope _envelope;
        private string _errMsg = String.Empty;
        private DatasetState _state = DatasetState.unknown;
        private ISpatialReference _sRef = null;
        private Dictionary<int, List<int>> _parentIds = new Dictionary<int, List<int>>();

        internal IWebProxy _proxy = null;
        internal gView.Interoperability.AGS.Proxy.MapServer _mapServer = null;
        internal MapDescription _mapDescription = null;

        public AGSDataset() { }

        public AGSDataset(string connection, string name)
        {
            _connection = connection;
            _name = name;

            _class = new AGSClass(this);
        }

        #region IFeatureDataset Member

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get { return _envelope; }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        #endregion

        #region IDataset Member

        public string ConnectionString
        {
            get
            {
                return _connection;
            }
            set
            {
                _connection = "server=" + ConfigTextStream.ExtractValue(value, "server") +
                                ";service=" + ConfigTextStream.ExtractValue(value, "service") +
                                ";user=" + ConfigTextStream.ExtractValue(value, "user") +
                                ";pwd=" + ConfigTextStream.ExtractValue(value, "pwd");
                _name = ConfigTextStream.ExtractValue(value, "service");
            }
        }

        public string DatasetGroupName
        {
            get { return "ESRI ArcGis Server"; }
        }

        public string DatasetName
        {
            get { return "ESRI ArcGis Server Map Service"; }
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
            return Open(null);
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public List<IDatasetElement> Elements
        {
            get
            {
                List<IDatasetElement> elements = new List<IDatasetElement>();
                if (_class != null)
                {
                    elements.Add(new DatasetElement(_class));
                }
                return elements;
            }
        }

        public string Query_FieldPrefix
        {
            get { return String.Empty; }
        }

        public string Query_FieldPostfix
        {
            get { return String.Empty; }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get { return null; }
        }

        public IDatasetElement this[string title]
        {
            get
            {
                if (_class != null) return new DatasetElement(_class);
                return null;
            }
        }

        public void RefreshClasses()
        {
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            _state = DatasetState.unknown;
        }

        #endregion

        #region IRequestDependentDataset Member

        public bool Open(gView.MapServer.IServiceRequestContext context)
        {
            if (_class == null) _class = new AGSClass(this);

            #region Parameters
            string server = ConfigTextStream.ExtractValue(ConnectionString, "server");
            string service = ConfigTextStream.ExtractValue(ConnectionString, "service");
            string user = ConfigTextStream.ExtractValue(ConnectionString, "user");
            string pwd = ConfigTextStream.ExtractValue(ConnectionString, "pwd");

            if ((user == "#" || user == "$") &&
                    context != null && context.ServiceRequest != null && context.ServiceRequest.Identity != null)
            {
                string roles = String.Empty;
                if (user == "#" && context.ServiceRequest.Identity.UserRoles != null)
                {
                    foreach (string role in context.ServiceRequest.Identity.UserRoles)
                    {
                        if (String.IsNullOrEmpty(role)) continue;
                        roles += "|" + role;
                    }
                }
                user = context.ServiceRequest.Identity.UserName + roles;
                pwd = context.ServiceRequest.Identity.HashedPassword;
            }
            #endregion

            try
            {
                _proxy = ProxySettings.Proxy(server);

                _themes.Clear();
                _parentIds.Clear();

                _mapServer = new gView.Interoperability.AGS.Proxy.MapServer(service);
                _mapServer.Proxy = gView.Framework.Web.ProxySettings.Proxy(service);
                MapServerInfo msi = _mapServer.GetServerInfo(_mapServer.GetDefaultMapName());
                _mapDescription = msi.DefaultMapDescription;

                MapLayerInfo[] mapLayerInfos = msi.MapLayerInfos;
                foreach (MapLayerInfo layerInfo in mapLayerInfos)
                {
                    if (layerInfo.Extent is EnvelopeN)
                    {
                        EnvelopeN env = (EnvelopeN)layerInfo.Extent;
                        if (_envelope == null)
                            _envelope = new gView.Framework.Geometry.Envelope(env.XMin, env.YMin, env.XMax, env.YMax);
                        else
                            _envelope.Union(new gView.Framework.Geometry.Envelope(env.XMin, env.YMin, env.XMax, env.YMax));
                    }

                    CalcParentLayerIds(mapLayerInfos, layerInfo.LayerID);
                    IClass themeClass = null;
                    IWebServiceTheme theme = null;
                    LayerDescription ld = LayerDescriptionById(layerInfo.LayerID);
                    if (ld == null)
                        continue;

                    if (layerInfo.LayerType == "Feature Layer")
                    {
                        #region Geometry Type (Point, Line, Polygon)
                        geometryType geomType = geometryType.Unknown;
                        if (layerInfo.Fields != null)
                        {
                            foreach (Proxy.Field fieldInfo in layerInfo.Fields.FieldArray)
                            {
                                if (fieldInfo.Type == esriFieldType.esriFieldTypeGeometry &&
                                    fieldInfo.GeometryDef != null)
                                {
                                    switch (fieldInfo.GeometryDef.GeometryType)
                                    {
                                        case esriGeometryType.esriGeometryMultipoint:
                                        case esriGeometryType.esriGeometryPoint:
                                            geomType = geometryType.Point;
                                            break;
                                        case esriGeometryType.esriGeometryPolyline:
                                            geomType = geometryType.Polyline;
                                            break;
                                        case esriGeometryType.esriGeometryPolygon:
                                            geomType = geometryType.Polygon;
                                            break;
                                        case esriGeometryType.esriGeometryMultiPatch:
                                            break;
                                    }
                                }
                            }
                        }
                        #endregion

                        themeClass = new AGSThemeFeatureClass(this, layerInfo, geomType);
                        theme = LayerFactory.Create(themeClass, _class as IWebServiceClass) as IWebServiceTheme;
                        if (theme == null) continue;

                    }
                    else if (layerInfo.LayerType == "Raster Layer" ||
                        layerInfo.LayerType == "Raster Catalog Layer")
                    {
                        themeClass = new AGSThemeRasterClass(this, layerInfo);
                        theme = LayerFactory.Create(themeClass, _class as IWebServiceClass) as IWebServiceTheme;
                        if (theme == null) continue;
                    }
                    else
                    {
                        MapLayerInfo parentLayer = MapLayerInfoById(mapLayerInfos, layerInfo.ParentLayerID);
                        if (parentLayer != null && parentLayer.LayerType == "Annotation Layer")
                        {
                            themeClass = new AGSThemeFeatureClass(this, layerInfo, geometryType.Polygon);
                            theme = LayerFactory.Create(themeClass, _class as IWebServiceClass) as IWebServiceTheme;
                            if (theme == null) continue;
                        }
                    }
                    if (theme != null)
                    {
                        theme.MinimumScale = layerInfo.MaxScale;
                        theme.MaximumScale = layerInfo.MinScale;
                        theme.Visible = ld.Visible;
                        _themes.Add(theme);
                    }
                }

                _state = DatasetState.opened;
                return true;
            }
            catch (Exception ex)
            {
                _state = DatasetState.unknown;
                _errMsg = ex.Message;
                return false;
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            this.ConnectionString = (string)stream.Load("ConnectionString", "");

            _class = new AGSClass(this);
            Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("ConnectionString", this.ConnectionString);
        }

        #endregion

        #region Helper
        internal LayerDescription LayerDescriptionById(int id)
        {
            if (_mapDescription == null || _mapDescription.LayerDescriptions == null)
                return null;
            LayerDescription[] lds = _mapDescription.LayerDescriptions;

            foreach (LayerDescription ld in lds)
            {
                if (ld.LayerID == id)
                    return ld;
            }
            return null;
        }

        internal MapLayerInfo MapLayerInfoById(MapLayerInfo[] mapLayerInfos, int layerId)
        {
            foreach (MapLayerInfo mapLayerInfo in mapLayerInfos)
            {
                if (mapLayerInfo.LayerID == layerId)
                    return mapLayerInfo;
            }
            return null;
        }

        private void CalcParentLayerIds(MapLayerInfo[] mapLayerInfos, int layerId)
        {
            MapLayerInfo layer = MapLayerInfoById(mapLayerInfos, layerId);
            if (layer == null || layer.ParentLayerID < 0)
                return;

            List<int> parentIds = new List<int>();
            while (layer != null)
            {
                parentIds.Add(layer.ParentLayerID);
                layer = MapLayerInfoById(mapLayerInfos, layer.ParentLayerID);
            }
            _parentIds[layerId] = parentIds;
        }

        internal int[] ParentLayerIds(int layerId)
        {
            if (_parentIds.ContainsKey(layerId))
                return _parentIds[layerId].ToArray();
            return new int[] { };
        }
        #endregion
    }
}
