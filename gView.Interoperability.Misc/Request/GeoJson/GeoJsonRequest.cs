using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.MapServer;
using gView.Framework.system;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Interoperability.Misc.Request.GeoJson
{
    [gView.Framework.system.RegisterPlugIn("d293cacf-31fb-4c60-af07-ec7d7c0a7a1b")]
    public class GeoJsonRequest : IServiceRequestInterpreter
    {
        private IMapServer _mapServer = null;
        private bool _useTOC = false;

        #region IServiceRequestInterpreter Member

        public void OnCreate(IMapServer mapServer)
        {
            _mapServer = mapServer;
        }

        public void Request(IServiceRequestContext context)
        {
            if (context == null || context.ServiceRequest == null)
                return;

            if (_mapServer == null)
            {
                return;
            }

            MiscParameterDescriptor parameters = new MiscParameterDescriptor();
            if (!parameters.ParseParameters(context.ServiceRequest.Request.Split('&')))
            {
                _mapServer.Log("Invalid Parameters", loggingMethod.error, context.ServiceRequest.Request);
                return;
            }

            using (IServiceMap map = context.ServiceMap) // _mapServer[context];
            {
                if (map == null)
                {
                    _mapServer.Log("Invalid Map", loggingMethod.error, context.ServiceRequest.Request);
                    return;
                }

                QueryFilter filter = parameters.BBOX != null ? new SpatialFilter() : new QueryFilter();
                filter.SubFields = "*";
                if (parameters.BBOX != null)
                    ((SpatialFilter)filter).Geometry = parameters.BBOX;
                if (!String.IsNullOrEmpty(parameters.SRS))
                {
                    ISpatialReference sRef = SpatialReference.FromID(parameters.SRS);
                    filter.FeatureSpatialReference = sRef;
                    if (filter is SpatialFilter)
                        ((SpatialFilter)filter).FilterSpatialReference = sRef;
                }
                // Get Layers
                List<ILayer> queryLayers = new List<ILayer>();
                foreach (string l in parameters.LAYERS)
                {
                    if (l == String.Empty || l[0] != 'c') continue;

                    MapServerHelper.Layers layers = MapServerHelper.FindMapLayers(map, _useTOC, l.Substring(1, l.Length - 1));
                    if (layers == null) continue;

                    foreach (ILayer layer in layers)
                    {
                        queryLayers.Add(layer);
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{'type':'FeatureCollection','features':[");
                foreach (ILayer layer in queryLayers)
                {
                    if (layer is IFeatureLayer && ((IFeatureLayer)layer).FeatureClass != null)
                    {
                        using (IFeatureCursor cursor = ((IFeatureLayer)layer).FeatureClass.Search(filter) as IFeatureCursor)
                        {
                            string json = gView.Framework.OGC.GeoJson.GeoJson.ToGeoJsonFeatures(cursor, parameters.MaxFeatures);
                            sb.Append(json);
                        }
                    }
                }
                sb.Append("]}");

                context.ServiceRequest.Response = sb.ToString();
            }
        }

        public string IntentityName
        {
            get { return "geojson"; }
        }

        public InterpreterCapabilities Capabilities
        {
            get
            {
                return new InterpreterCapabilities(new InterpreterCapabilities.Capability[]{
                    new InterpreterCapabilities.SimpleCapability("GeoJson","{onlineresource}LAYERS=<List of Layers>&BBOX=<optional>&SRS=<optional>","1.0")
                });
            }
        }

        #endregion
    }
}
