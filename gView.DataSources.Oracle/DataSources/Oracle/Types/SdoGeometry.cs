using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Types;
using Oracle.ManagedDataAccess.Client;
using gView.Framework.Geometry;

namespace gView.DataSources.OracleGeometry.Types
{
    public static class SdoGeometryTypes
    {
        // Oracle Documentation for SDO_ETYPE - SIMPLE
        // Point//Line//Polygon//exterior counterclockwise - polygon ring = 1003//interior clockwise  polygon ring = 2003
        public enum ETYPE_SIMPLE
        {
            POINT = 1,
            LINE = 2,
            POLYGON = 3,
            POLYGON_EXTERIOR = 1003,
            POLYGON_INTERIOR = 2003
        }

        // Oracle Documentation for SDO_ETYPE - COMPOUND
        // 1005: exterior polygon ring (must be specified in counterclockwise order)
        // 2005: interior polygon ring (must be specified in clockwise order)
        public enum ETYPE_COMPOUND
        {
            FOURDIGIT = 4,
            POLYGON_EXTERIOR = 1005,
            POLYGON_INTERIOR = 2005
        }

        // Oracle Documentation for SDO_GTYPE.
        // This represents the last two digits in a GTYPE, where the first item is dimension(ality) and the second is LRS
        public enum GTYPE
        {
            UNKNOWN_GEOMETRY = 00,
            POINT = 01,
            LINE = 02,
            CURVE = 02,
            POLYGON = 03,
            COLLECTION = 04,
            MULTIPOINT = 05,
            MULTILINE = 06,
            MULTICURVE = 06,
            MULTIPOLYGON = 07
        }

        public enum DIMENSION
        {
            DIM2D = 2,
            DIM3D = 3,
            LRS_DIM3 = 3,
            LRS_DIM4 = 4
        }
    }

    public static class SdoGeometry
    {
        // https://docs.oracle.com/cd/B10501_01/appdev.920/a96630/sdo_objrelschema.htm
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        #region FromGeometry

        public static string FromGeometry(IGeometry geometry, int? srs = null)
        {
            if (geometry is IPoint)
                return FromPointGeometry((IPoint)geometry, srs);
            if (geometry is IPolyline)
                return FromPolylineGeometry((IPolyline)geometry, srs);
            if (geometry is IPolygon)
                return FromPolygonGeometry((IPolygon)geometry, srs);

            return "NULL";
        }

        #region Helper

        private static string FromPointGeometry(IPoint point, int? srs = null)
        {
            string SDO_POINT = point.X.ToString(_nhi) + "," + point.Y.ToString(_nhi);
            //string SDO_ELEM_INFO = "NULL";
            //string SDO_ORDINATES = "NULL";

            return ToGeometryString(SdoGeometryTypes.GTYPE.POINT, srs, SDO_POINT);
        }

        private static string FromPolylineGeometry(IPolyline polyline, int? srs = null)
        {
            StringBuilder SDO_ELEMENT_INFO = new StringBuilder();
            StringBuilder SDO_ORDINATES = new StringBuilder();

            int pathCount = 0, offset = 1;
            for (int p = 0, to = polyline.PathCount; p < pathCount; p++)
            {
                var path = polyline[p];

                var pointCount = path.PointCount;
                if (pointCount < 2)
                    continue;

                pathCount++;

                if (pathCount > 0) SDO_ELEMENT_INFO.Append(",");
                SDO_ELEMENT_INFO.Append(offset);
                SDO_ELEMENT_INFO.Append(",");
                SDO_ELEMENT_INFO.Append(SdoGeometryTypes.ETYPE_SIMPLE.LINE);
                SDO_ELEMENT_INFO.Append(",");
                SDO_ELEMENT_INFO.Append("1");   // Connected by Strait lines

                for (int i = 0; i < pointCount; i++)
                {
                    var point = path[i];
                    if (offset > 1)
                        SDO_ORDINATES.Append(",");

                    SDO_ORDINATES.Append(point.X.ToString(_nhi));
                    SDO_ORDINATES.Append(",");
                    SDO_ORDINATES.Append(point.Y.ToString(_nhi));

                    offset++;
                }
            }

            if (pathCount == 0)
                return "NULL";

            return ToGeometryString(pathCount > 1 ? SdoGeometryTypes.GTYPE.MULTILINE : SdoGeometryTypes.GTYPE.LINE, srs,
                SDO_ELEM_INFO: SDO_ELEMENT_INFO.ToString(),
                SDO_ORDINATES: SDO_ORDINATES.ToString());
        }

        private static string FromPolygonGeometry(IPolygon polygon, int? srs = 0)
        {
            StringBuilder SDO_ELEMENT_INFO = new StringBuilder();
            StringBuilder SDO_ORDINATES = new StringBuilder();

            polygon.VerifyHoles();

            int ringCount = 0, offset = 1;
            for (int r = 0, to = polygon.RingCount; r < ringCount; r++)
            {
                var ring = polygon[r];

                var pointCount = ring.PointCount;
                if (pointCount < 2)
                    continue;

                ringCount++;

                if (ringCount > 0) SDO_ELEMENT_INFO.Append(",");
                SDO_ELEMENT_INFO.Append(offset);
                SDO_ELEMENT_INFO.Append(",");
                SDO_ELEMENT_INFO.Append(ring is IHole ? SdoGeometryTypes.ETYPE_SIMPLE.POLYGON_INTERIOR : SdoGeometryTypes.ETYPE_SIMPLE.POLYGON_EXTERIOR);
                SDO_ELEMENT_INFO.Append(",");
                SDO_ELEMENT_INFO.Append("1");   // Connected by Strait lines

                for (int i = 0; i < pointCount; i++)
                {
                    var point = ring[i];
                    if (offset > 1)
                        SDO_ORDINATES.Append(",");

                    SDO_ORDINATES.Append(point.X.ToString(_nhi));
                    SDO_ORDINATES.Append(",");
                    SDO_ORDINATES.Append(point.Y.ToString(_nhi));

                    offset++;
                }
            }

            if (ringCount == 0)
                return "NULL";

            return ToGeometryString(ringCount > 1 ? SdoGeometryTypes.GTYPE.MULTIPOLYGON : SdoGeometryTypes.GTYPE.POLYGON, srs,
                SDO_ELEM_INFO: SDO_ELEMENT_INFO.ToString(),
                SDO_ORDINATES: SDO_ORDINATES.ToString());
        }

        private static string ToGeometryString(SdoGeometryTypes.GTYPE SDO_GTYPE, int? srs = null, string SDO_POINT = null, string SDO_ELEM_INFO = null, string SDO_ORDINATES = null)
        {
            return "MDSYS.SDO_GEOMETRY(" + SdoGeometryTypes.DIMENSION.DIM2D + "0" + SDO_GTYPE.ToString() +
                "," + srs != null ? srs.Value.ToString() : "NULL" +
                "," + SDO_POINT != null ? "MDSYS.SDO_POINT_TYPE(" + SDO_POINT + ")" : "NULL" +
                "," + SDO_ELEM_INFO != null ? "MDSYS.SDO_ELEM_INFO_ARRAY(" + SDO_ELEM_INFO + ")" : "NULL" +
                "," + SDO_ORDINATES != null ? " MDSYS.SDO_ORDINATE_ARRAY(" + SDO_ORDINATES + ")" : "NULL" + ")";
        }

        #endregion

        #endregion

        #region ToGeometry

        //public int PropertiesFromGTYPE()
        //{
        //    if ((int)this.SdoGtype != 0)
        //    {
        //        int v = (int)this.SdoGtype;
        //        int dim = v / 1000;
        //        this.Dimensionality = dim;
        //        v -= dim * 1000;
        //        int lrsDim = v / 100;
        //        this.LRS = lrsDim;
        //        v -= lrsDim * 100;
        //        this.GeometryType = v;
        //        return (this.Dimensionality * 1000) + (this.LRS * 100) + this.GeometryType;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}

        #endregion
    }
}
