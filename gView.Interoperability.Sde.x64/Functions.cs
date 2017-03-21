using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.SDEWrapper.x64
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Pointer2Pointer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public System.IntPtr [] pointer;
    }

    public class Functions
    {
        public static string[] TableNames(SE_CONNECTION_64 connection)
        {
            try
            {
                unsafe
                {
                    Pointer2Pointer ptr = new Pointer2Pointer();
                    System.Int64 num = 0;


                    if (Wrapper92_64.SE_registration_get_info_list(connection, ref ptr, ref num) != 0) return null;
                    IntPtr* reginfo = (System.IntPtr*)ptr.pointer[0];

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        Int64 result = Wrapper92_64.SE_reginfo_has_layer(reginfo[i]);
                        if (result != 0)
                        {
                            byte[] buffer = new byte[CONST.SE_QUALIFIED_TABLE_NAME];
                            Wrapper92_64.SE_reginfo_get_table_name(reginfo[i], buffer);
                            string table = System.Text.Encoding.ASCII.GetString(buffer).Replace("\0", "");
                            sb.Append(table);

                            /*
                            buffer = new byte[CONST.SE_MAX_DESCRIPTION_LEN];
                            Wrapper92_64.SE_reginfo_get_description(reginfo[i], buffer);
                            string descr = System.Text.Encoding.ASCII.GetString(buffer).Replace("\0", "");
                            sb.Append(" ("+descr+")" + ";");
                             * */
                        }
                        Wrapper92_64.SE_reginfo_free(reginfo[i]);
                    }
                    return sb.ToString().Split(';');

                }
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public delegate void GetLayerInfosCallBack(IntPtr layerInfo);
        public static void GetLayerInfos(SE_CONNECTION_64 connection, GetLayerInfosCallBack call)
        {
            if (call == null) return;
            try
            {
                unsafe
                {
                    //Pointer2Pointer ptr = new Pointer2Pointer();
                    //System.Int32 num = 0;

                    //System.Int32 status = Wrapper92_64.SE_layer_get_info_list(connection,ref ptr, ref num);
                    //if (status != 0)
                    //{
                    //    return;
                    //}
                    //IntPtr* layerInfo = (System.IntPtr*)ptr.pointer[0];
                    
                    //for (int i = 0; i < num; i++)
                    //{
                    //    call(layerInfo[i]);
                    //}
                    //if (num > 0) Wrapper92_64.SE_layer_free_info_list(num, ptr.pointer[0]);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static System.Int64 SE_GenerateGeometry(SE_SHAPE_64 shape, IGeometry geometry, SE_ENVELOPE maxExtent)
        {
            if (geometry == null) return -1;

            unsafe
            {
                SE_POINT* points = null;
                System.Int32* parts = null;
                switch (geometry.GeometryType)
                {
                    case geometryType.Envelope:
                        SE_ENVELOPE seEnvelope = new SE_ENVELOPE();
                        IEnvelope env = geometry.Envelope;
                        seEnvelope.minx = Math.Max(env.minx, maxExtent.minx);
                        seEnvelope.miny = Math.Max(env.miny, maxExtent.miny);
                        seEnvelope.maxx = Math.Min(env.maxx, maxExtent.maxx);
                        seEnvelope.maxy = Math.Min(env.maxy, maxExtent.maxy);

                        if (seEnvelope.minx == seEnvelope.maxx && seEnvelope.miny == seEnvelope.maxy)
                        {
                            /* fudge a rectangle so we have a valid one for generate_rectangle */
                            /* FIXME: use the real shape for the query and set the filter_type 
                               to be an appropriate type */
                            seEnvelope.minx = seEnvelope.minx - 0.001;
                            seEnvelope.maxx = seEnvelope.maxx + 0.001;
                            seEnvelope.miny = seEnvelope.miny - 0.001;
                            seEnvelope.maxy = seEnvelope.maxy + 0.001;
                        }
                        return Wrapper92_64.SE_shape_generate_rectangle(ref seEnvelope, shape);
                    case geometryType.Point:
                        points = (SE_POINT*)Marshal.AllocHGlobal(sizeof(SE_POINT) * 1);
                        points[0].x = ((IPoint)geometry).X;
                        points[0].y = ((IPoint)geometry).Y;
                        return Wrapper92_64.SE_shape_generate_point(1, (IntPtr)points, (IntPtr)null, (IntPtr)null, shape);
                    case geometryType.Polyline:
                        IPointCollection col1 = gView.Framework.SpatialAlgorithms.Algorithm.GeometryPoints(geometry, false);
                        IPolyline polyline = (IPolyline)geometry;
                        points = (SE_POINT*)Marshal.AllocHGlobal(sizeof(SE_POINT) * (col1.PointCount));
                        parts = (Int32*)Marshal.AllocHGlobal(sizeof(Int32) * polyline.PathCount);

                        int pos1 = 0;
                        for (int i = 0; i < polyline.PathCount; i++)
                        {
                            parts[i] = pos1;
                            IPath path = polyline[i];
                            if (path.PointCount == 0) continue;
                            for (int p = 0; p < path.PointCount; p++)
                            {
                                points[pos1].x = path[p].X;
                                points[pos1].y = path[p].Y;
                                pos1++;
                            }
                        }

                        return Wrapper92_64.SE_shape_generate_line(pos1, polyline.PathCount, (IntPtr)parts, (IntPtr)points, (IntPtr)null, (IntPtr)null, shape);
                    case geometryType.Polygon:
                        IPointCollection col2 = gView.Framework.SpatialAlgorithms.Algorithm.GeometryPoints(geometry, false);
                        IPolygon polygon = (IPolygon)geometry;
                        points = (SE_POINT*)Marshal.AllocHGlobal(sizeof(SE_POINT) * (col2.PointCount + polygon.RingCount));
                        parts = (Int32*)Marshal.AllocHGlobal(sizeof(Int32) * polygon.RingCount);
                        
                        int pos2 = 0;
                        for (int i = 0; i < polygon.RingCount; i++)
                        {
                            parts[i] = pos2;
                            IRing ring= polygon[i];
                            if (ring.PointCount == 0) continue;
                            for (int p = 0; p < ring.PointCount; p++)
                            {
                                points[pos2].x = ring[p].X;
                                points[pos2].y = ring[p].Y;
                                pos2++;
                            }
                            points[pos2].x = ring[0].X;
                            points[pos2].y = ring[0].Y;
                            pos2++;
                        }

                        return Wrapper92_64.SE_shape_generate_polygon(pos2, polygon.RingCount, (IntPtr)parts, (IntPtr)points, (IntPtr)null, (IntPtr)null, shape);
                    case geometryType.Aggregate:
                        if (((AggregateGeometry)geometry).GeometryCount == 1)
                        {
                            return SE_GenerateGeometry(shape, ((AggregateGeometry)geometry)[0], maxExtent);
                        }
                        //else
                        //{
                        //    Polygon polygon = new Polygon();
                        //    for (int i = 0; i < ((AggregateGeometry)geometry).GeometryCount; i++)
                        //    {
                        //        IGeometry g = ((AggregateGeometry)geometry)[i];
                        //        if (g is IPolygon)
                        //        {
                        //            for (int p = 0; p < ((IPolygon)g).RingCount; p++)
                        //                polygon.AddRing(((Polygon)g)[p]);
                        //        }
                        //    }
                        //    if (polygon.RingCount > 0) return SE_GenerateGeometry(shape, polygon, maxExtent);
                        //}
                        return -1;
                }
            }

            return -1;
        }

        public static string GetASCIIString(byte[] bytes)
        {
            string str = System.Text.Encoding.ASCII.GetString(bytes);
            int pos = str.IndexOf("\0");
            if (pos != -1)
                str = str.Substring(0, pos);
            return str;
        }
    }
}
