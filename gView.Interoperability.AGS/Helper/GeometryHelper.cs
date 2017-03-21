using System;
using System.Collections.Generic;
using System.Text;
using gView.Interoperability.AGS.Proxy;

namespace gView.Interoperability.AGS.Helper
{
    public class GeometryHelper
    {
        #region FromWebGIS
        public static gView.Interoperability.AGS.Proxy.Geometry FromWebGIS(gView.Framework.Geometry.IGeometry geometry)
        {
            if (geometry == null) return null;

            if (geometry is gView.Framework.Geometry.IEnvelope)
            {
                EnvelopeN env = new EnvelopeN();
                env.XMin = ((gView.Framework.Geometry.IEnvelope)geometry).minx;
                env.YMin = ((gView.Framework.Geometry.IEnvelope)geometry).miny;
                env.XMax = ((gView.Framework.Geometry.IEnvelope)geometry).maxx;
                env.YMax = ((gView.Framework.Geometry.IEnvelope)geometry).maxy;

                return env;
            }
            if (geometry is gView.Framework.Geometry.IPoint)
            {
                return ToPoint((gView.Framework.Geometry.IPoint)geometry);
            }
            if (geometry is gView.Framework.Geometry.IMultiPoint)
            {
                return ToMultipoint((gView.Framework.Geometry.IMultiPoint)geometry);
            }
            if (geometry is gView.Framework.Geometry.IPolyline)
            {
                return ToPolyline((gView.Framework.Geometry.IPolyline)geometry);
            }
            if (geometry is gView.Framework.Geometry.IPolygon)
            {
                return ToPolygon((gView.Framework.Geometry.IPolygon)geometry);
            }
            return null;
        }

        private static PointN ToPoint(gView.Framework.Geometry.IPoint point)
        {
            if (point == null) return null;

            PointN p = new PointN();
            p.X = point.X;
            p.Y = point.Y;
            if (point.Z != 0.0) p.Z = point.Z;

            return p;
        }

        private static PointN[] ToPointArray(gView.Framework.Geometry.IPointCollection pointCollection)
        {
            return ToPointArray(pointCollection, false);
        }
        private static PointN[] ToPointArray(gView.Framework.Geometry.IPointCollection pointCollection, bool closeIt)
        {
            List<PointN> pColl = new List<PointN>();
            if (pointCollection != null)
            {
                for (int i = 0; i < pointCollection.PointCount; i++)
                {
                    pColl.Add(ToPoint(pointCollection[i]));
                }
            }

            if (closeIt && pColl.Count > 2)
            {
                if (Math.Abs(pColl[0].X - pColl[pColl.Count - 1].X) > double.Epsilon ||
                    Math.Abs(pColl[0].Y - pColl[pColl.Count - 1].Y) > double.Epsilon ||
                    Math.Abs(pColl[0].Z - pColl[pColl.Count - 1].Z) > double.Epsilon)
                {
                    pColl.Add(ToPoint(new gView.Framework.Geometry.Point(
                        pColl[0].X, pColl[0].Y, pColl[0].Z)));
                }
            }
            return pColl.ToArray();
        }

        private static MultipointN ToMultipoint(gView.Framework.Geometry.IMultiPoint multipoint)
        {
            MultipointN multi = new MultipointN();
            multi.PointArray = ToPointArray(multipoint);
            return multi;
        }
        private static PolylineN ToPolyline(gView.Framework.Geometry.IPolyline polyline)
        {
            PolylineN pline = new PolylineN();

            if (polyline != null)
            {
                List<Path> paths = new List<Path>();
                for (int i = 0; i < polyline.PathCount; i++)
                {
                    Path path = new Path();
                    path.PointArray = ToPointArray(polyline[i]);
                    paths.Add(path);
                }
                pline.PathArray = paths.ToArray();
            }
            return pline;
        }

        private static PolygonN ToPolygon(gView.Framework.Geometry.IPolygon polygon)
        {
            PolygonN poly = new PolygonN();

            if (polygon != null)
            {
                //polygon.VerifyHoles();
                List<Ring> rings = new List<Ring>();
                for (int i = 0; i < polygon.RingCount; i++)
                {
                    Ring ring = new Ring();
                    ring.PointArray = ToPointArray(polygon[i], true);
                    rings.Add(ring);
                }
                poly.RingArray = rings.ToArray();
            }
            return poly;
        }
        #endregion

        #region ToWebGIS
        public static gView.Framework.Geometry.IGeometry ToGView(gView.Interoperability.AGS.Proxy.Geometry geometry)
        {
            if (geometry == null) return null;

            if (geometry is EnvelopeN)
            {
                return new gView.Framework.Geometry.Envelope(
                    ((EnvelopeN)geometry).XMin,
                    ((EnvelopeN)geometry).YMin,
                    ((EnvelopeN)geometry).XMax,
                    ((EnvelopeN)geometry).YMax);
            }
            else if (geometry is PointN)
            {
                return ToPoint((PointN)geometry);
            }
            else if (geometry is PolylineN)
            {
                return ToPolyline((PolylineN)geometry);
            }
            else if (geometry is PolygonN)
            {
                return ToPolygon((PolygonN)geometry);
            }

            return null;
        }

        private static gView.Framework.Geometry.IPoint ToPoint(PointN point)
        {
            return new gView.Framework.Geometry.Point(point.X, point.Y, point.Z);
        }

        private static gView.Framework.Geometry.IPath ToPath(Path path, ref bool complex)
        {
            gView.Framework.Geometry.Path p = new gView.Framework.Geometry.Path();
            if (path != null && path.PointArray != null)
            {
                foreach (Point point in path.PointArray)
                {
                    if (point is PointN)
                        p.AddPoint(ToPoint((PointN)point));
                }
            }
            else if (path != null && path.SegmentArray != null)
            {
                complex = true;
                foreach (Segment segment in path.SegmentArray)
                {
                    #region Line
                    if (segment is Line &&
                        (((Line)segment).FromPoint is PointN &&
                         ((Line)segment).ToPoint is PointN))
                    {
                        gView.Framework.Geometry.IPoint p1 = ToPoint((PointN)((Line)segment).FromPoint);
                        gView.Framework.Geometry.IPoint p2 = ToPoint((PointN)((Line)segment).ToPoint);

                        if (p.PointCount == 0)
                        {
                            p.AddPoint(p1);
                            p.AddPoint(p2);
                        }
                        else
                        {
                            if (p[p.PointCount - 1].Equals(p1))
                            {
                                p.AddPoint(p2);
                            }
                            else
                            {
                                p.AddPoint(p1);
                                p.AddPoint(p2);
                            }
                        }
                    }
                    #endregion
                    #region CircularArc
                    else if (segment is CircularArc &&
                            (((CircularArc)segment).FromPoint is PointN &&
                             ((CircularArc)segment).ToPoint is PointN))
                    {
                        gView.Framework.Geometry.IPoint p1 = ToPoint((PointN)((CircularArc)segment).FromPoint);
                        gView.Framework.Geometry.IPoint p2 = ToPoint((PointN)((CircularArc)segment).ToPoint);

                        if (p.PointCount == 0)
                        {
                            p.AddPoint(p1);
                            p.AddPoint(p2);
                        }
                        else
                        {
                            if (p[p.PointCount - 1].Equals(p1))
                            {
                                p.AddPoint(p2);
                            }
                            else
                            {
                                p.AddPoint(p1);
                                p.AddPoint(p2);
                            }
                        }
                    }
                    #endregion
                }
            }
            return p;
        }

        private static gView.Framework.Geometry.IPolyline ToPolyline(PolylineN polyline)
        {
            gView.Framework.Geometry.Polyline p = new gView.Framework.Geometry.Polyline();
            bool complex = false;
            if (polyline != null && polyline.PathArray != null)
            {
                foreach (Path path in polyline.PathArray)
                {
                    p.AddPath(ToPath(path, ref complex));
                }
            }
            //p.IsComplex = complex;
            return p;
        }

        private static gView.Framework.Geometry.IRing ToRing(Ring ring, ref bool complex)
        {
            gView.Framework.Geometry.Ring r = new gView.Framework.Geometry.Ring();
            if (ring != null && ring.PointArray != null)
            {
                foreach (Point point in ring.PointArray)
                {
                    if (point is PointN)
                        r.AddPoint(ToPoint((PointN)point));
                }
            }
            else if (ring != null && ring.SegmentArray != null)
            {
                complex = true;
                foreach (Segment segment in ring.SegmentArray)
                {
                    #region Line
                    if (segment is Line &&
                        (((Line)segment).FromPoint is PointN &&
                         ((Line)segment).ToPoint is PointN))
                    {
                        gView.Framework.Geometry.IPoint p1 = ToPoint((PointN)((Line)segment).FromPoint);
                        gView.Framework.Geometry.IPoint p2 = ToPoint((PointN)((Line)segment).ToPoint);

                        if (r.PointCount == 0)
                        {
                            r.AddPoint(p1);
                            r.AddPoint(p2);
                        }
                        else
                        {
                            if (r[r.PointCount - 1].Equals(p1))
                            {
                                r.AddPoint(p2);
                            }
                            else
                            {
                                r.AddPoint(p1);
                                r.AddPoint(p2);
                            }
                        }
                    }
                    #endregion
                    #region CircularArc
                    else if (segment is CircularArc &&
                            (((CircularArc)segment).FromPoint is PointN &&
                             ((CircularArc)segment).ToPoint is PointN))
                    {
                        gView.Framework.Geometry.IPoint p1 = ToPoint((PointN)((CircularArc)segment).FromPoint);
                        gView.Framework.Geometry.IPoint p2 = ToPoint((PointN)((CircularArc)segment).ToPoint);

                        if (r.PointCount == 0)
                        {
                            r.AddPoint(p1);
                            r.AddPoint(p2);
                        }
                        else
                        {
                            if (r[r.PointCount - 1].Equals(p1))
                            {
                                r.AddPoint(p2);
                            }
                            else
                            {
                                r.AddPoint(p1);
                                r.AddPoint(p2);
                            }
                        }
                    }
                    #endregion
                }
            }
            return r;
        }

        private static gView.Framework.Geometry.IPolygon ToPolygon(PolygonN polygon)
        {
            gView.Framework.Geometry.Polygon p = new gView.Framework.Geometry.Polygon();
            bool complex = false;
            if (polygon != null && polygon.RingArray != null)
            {
                foreach (Ring ring in polygon.RingArray)
                {
                    p.AddRing(ToRing(ring, ref complex));
                }
            }
            //p.IsComplex = complex;
            return p;
        }
        #endregion
    }
}
