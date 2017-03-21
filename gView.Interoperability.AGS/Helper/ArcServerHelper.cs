using System;
using System.Collections.Generic;
using System.Text;
using gView.Interoperability.AGS.Proxy;
using System.Net;

namespace gView.Interoperability.AGS.Helper
{
    public class ArcServerHelper
    {
        #region Catalog
        public static string CatalogUrl(string server)
        {
            if (server.ToLower().StartsWith("http://") ||
                server.ToLower().StartsWith("https://")) return server;

            return "http://" + server + "/ArcGIS/Services";
        }

        public static ServiceDescription[] MapServerServices(string server, IWebProxy proxy)
        {
            Catalog catalog = new Catalog(CatalogUrl(server));
            catalog.Proxy = proxy;
            //catalog.Credentials = System.Net.CredentialCache.DefaultCredentials;

            List<ServiceDescription> services = new List<ServiceDescription>();
            foreach (ServiceDescription serviceDescription in catalog.GetServiceDescriptions())
            {
                if (serviceDescription.Type == "MapServer")
                {
                    services.Add(serviceDescription);
                }
            }

            return services.ToArray();
        }
        #endregion

        #region ServiceInfo
        public static ServiceDescription GetMapServerServiceDescription(string server, string service, WebProxy proxy)
        {
            foreach (ServiceDescription serviceDescription in MapServerServices(server, proxy))
            {
                if (serviceDescription.Name == service)
                    return serviceDescription;
            }
            return null;
        }

        public static MapServerInfo GetMapServerInfo(string serviceUrl)
        {
            gView.Interoperability.AGS.Proxy.MapServer mapServer = new gView.Interoperability.AGS.Proxy.MapServer(serviceUrl);
            return mapServer.GetServerInfo(mapServer.GetDefaultMapName());
        }

        public static MapDescription GetMapDescription(string serviceUrl)
        {
            return GetMapServerInfo(serviceUrl).DefaultMapDescription;
        }

        public static LayerDescription GetLayerDescription(MapDescription md, int layerID)
        {
            if (md == null) return null;

            foreach (LayerDescription ld in md.LayerDescriptions)
            {
                if (ld.LayerID == layerID)
                    return ld;
            }

            return null;
        }
        #endregion

        #region Create Objects
        public static RgbColor RgbColor(System.Drawing.Color col)
        {
            RgbColor c = new RgbColor();
            c.Red = col.R;
            c.Green = col.G;
            c.Blue = col.B;
            c.AlphaValue = col.A;

            return c;
        }

        public static EnvelopeN EnvelopeN(gView.Framework.Geometry.IEnvelope env)
        {
            if (env == null) return null;

            EnvelopeN e = new EnvelopeN();
            e.XMin = env.minx;
            e.YMin = env.miny;
            e.XMax = env.maxx;
            e.YMax = env.maxy;
            return e;
        }

        public static QueryFilter QueryFilter(gView.Framework.Data.IQueryFilter filter, gView.Framework.Data.IFeatureClass fc)
        {
            if (filter is gView.Framework.Data.ISpatialFilter)
            {
                SpatialFilter sFilter = new SpatialFilter();
                sFilter.FilterGeometry = GeometryHelper.FromWebGIS(((gView.Framework.Data.ISpatialFilter)filter).Geometry);
                if (fc != null)
                    sFilter.GeometryFieldName = fc.ShapeFieldName;
                sFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sFilter.WhereClause = filter.WhereClause;
                sFilter.SubFields = filter.SubFields.Replace(" ", ",");

                //gView.Framework.Geometry.SpatialReference filterSRef = ((gView.Framework.Data.ISpatialFilter)filter).FilterSpatialReference as gView.Framework.Geometry.SpatialReference;
                //if (filterSRef != null && filterSRef.Id > 0)
                //{
                //    SpatialReference sref = (filterSRef.IsProjective == true ? (SpatialReference)(new ProjectedCoordinateSystem()) : (SpatialReference)(new GeographicCoordinateSystem()));
                //    sref.WKID = filterSRef.Id;
                //    sref.WKIDSpecified = true;

                //    if (sFilter.FilterGeometry is PointN)
                //        ((PointN)sFilter.FilterGeometry).SpatialReference = sref;
                //    else if (sFilter.FilterGeometry is PolylineN)
                //        ((PolylineN)sFilter.FilterGeometry).SpatialReference = sref;
                //    else if (sFilter.FilterGeometry is PolygonN)
                //        ((PolygonN)sFilter.FilterGeometry).SpatialReference = sref;
                //    else if (sFilter.FilterGeometry is MultipointN)
                //        ((MultipointN)sFilter.FilterGeometry).SpatialReference = sref;
                //    else if (sFilter.FilterGeometry is EnvelopeN)
                //        ((EnvelopeN)sFilter.FilterGeometry).SpatialReference = sref;
                //}

                //if (filter.FeatureSpatialReference != null && filter.FeatureSpatialReference.Id > 0)
                //{
                //    SpatialReference sref = (filter.FeatureSpatialReference.IsProjektive == true ? (SpatialReference)(new ProjectedCoordinateSystem()) : (SpatialReference)(new GeographicCoordinateSystem()));
                //    sref.WKID = filter.FeatureSpatialReference.Id;
                //    sref.WKIDSpecified = true;

                //    sFilter.OutputSpatialReference = sref;
                //    sFilter.SpatialReferenceFieldName = layer.ShapeFieldName;
                //}
                return sFilter;
            }
            else if (filter is gView.Framework.Data.IRowIDFilter)
            {
                QueryFilter qFilter = new QueryFilter();
                qFilter.WhereClause = ((gView.Framework.Data.IRowIDFilter)filter).RowIDWhereClause;
                if (!String.IsNullOrEmpty(filter.WhereClause))
                    qFilter.WhereClause += " AND " + filter.WhereClause;
                qFilter.SubFields = filter.SubFields.Replace(" ", ",");

                return qFilter;
            }
            else if (filter is gView.Framework.Data.IGlobalRowIDFilter)
            {
                QueryFilter qFilter = new QueryFilter();
                qFilter.WhereClause = ((gView.Framework.Data.IGlobalRowIDFilter)filter).RowIDWhereClause;
                if (!String.IsNullOrEmpty(filter.WhereClause))
                    qFilter.WhereClause += " AND " + filter.WhereClause;
                qFilter.SubFields = filter.SubFields.Replace(" ", ",");

                return qFilter;
            }
            else
            {
                QueryFilter qFilter = new QueryFilter();
                qFilter.WhereClause = filter.WhereClause;
                qFilter.SubFields = filter.SubFields.Replace(" ", ",");

                //if (filter.FeatureSpatialReference != null && filter.FeatureSpatialReference.Id > 0)
                //{
                //    SpatialReference sref = (filter.FeatureSpatialReference.IsProjective == true ? (SpatialReference)(new ProjectedCoordinateSystem()) : (SpatialReference)(new GeographicCoordinateSystem()));
                //    sref.WKID = filter.FeatureSpatialReference.;
                //    sref.WKIDSpecified = true;

                //    qFilter.OutputSpatialReference = sref;
                //}
                return qFilter;
            }

            return null;
        }
        #endregion
    }
}
