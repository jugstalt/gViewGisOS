namespace gView.Framework.SpatialAlgorithms
{
	using System;
	using System.Collections;
    using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Runtime.InteropServices;
    using gView.Framework.Geometry;

	internal enum ClipOperation
	{
		Difference   = 0,
		Intersection = 1,
		XOr          = 2,
		Union        = 3
	}
	
    internal enum BufferOperation 
    {
        Buffer_POINTS=0,                         
        Buffer_LINES =1
    }
	internal struct GeomVertex
	{
		public double X;
		public double Y;
		
		public GeomVertex( double x, double y )
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return "(" + X.ToString() + "," + Y.ToString() + ")";
		}
	}

	internal class GeomVertexList
	{
		public int          NofVertices;
		public GeomVertex[]     Vertex;
	
		public GeomVertexList()
		{
		}
		
		public GeomVertexList( PointF[] p )
		{
			NofVertices = p.Length;
			Vertex = new GeomVertex[NofVertices];
			for ( int i=0 ; i<p.Length ; i++ )
				Vertex[i] = new GeomVertex( (double)p[i].X, (double)p[i].Y );
		}

        public GeomVertexList(IPointCollection pColl)
        {
            pColl = Algorithm.RemoveDoubles(pColl);
            if (Algorithm.IsSelfIntersecting(pColl))
                return;

            NofVertices = pColl.PointCount;

            if (NofVertices > 1)
            {
                if (pColl[0].X != pColl[NofVertices - 1].X ||
                    pColl[0].Y != pColl[NofVertices - 1].Y)
                {
                    NofVertices++;
                }
            }

            Vertex = new GeomVertex[NofVertices];
            for (int i = 0; i < pColl.PointCount; i++)
                Vertex[i] = new GeomVertex(pColl[i].X, pColl[i].Y);

            if (NofVertices == pColl.PointCount + 1)
            {
                Vertex[NofVertices-1] = new GeomVertex(pColl[0].X, pColl[0].Y);
            }
        }

        public GraphicsPath ToGraphicsPath()
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLines( ToPoints() );
			return graphicsPath;
		}

        public IRing ToRing()
        {
            Ring ring = new Ring();
            for (int i = 0; i < NofVertices; i++)
                ring.AddPoint(new gView.Framework.Geometry.Point(Vertex[i].X, Vertex[i].Y));
            return ring;
        }

        public PointF[] ToPoints()
		{
			PointF[] vertexArray = new PointF[NofVertices];
			for ( int i=0 ; i<NofVertices ; i++ ) {
				vertexArray[i] = new PointF( (float)Vertex[i].X, (float)Vertex[i].Y );
			}
			return vertexArray;
		}
		
		public GraphicsPath TristripToGraphicsPath()
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			
			for ( int i=0 ; i<NofVertices-2 ; i++ ) {
				graphicsPath.AddPolygon( new PointF[3]{ new PointF( (float)Vertex[i].X,   (float)Vertex[i].Y ),
				                                        new PointF( (float)Vertex[i+1].X, (float)Vertex[i+1].Y ),
				                                        new PointF( (float)Vertex[i+2].X, (float)Vertex[i+2].Y )  }  );
			}
			
			return graphicsPath;
		}

        public void TristripToPolyons(ref List<IPolygon> polygons)
        {
            if (polygons == null) return;

            for (int i = 0; i < NofVertices - 2; i++)
            {
                Polygon poly = new Polygon();
                Ring ring = new Ring();

                ring.AddPoint(new gView.Framework.Geometry.Point(
                    Vertex[i].X, Vertex[i].Y));
                ring.AddPoint(new gView.Framework.Geometry.Point(
                   Vertex[i + 1].X, Vertex[i + 1].Y));
                ring.AddPoint(new gView.Framework.Geometry.Point(
                   Vertex[i + 2].X, Vertex[i + 2].Y));

                poly.AddRing(ring);
                polygons.Add(poly);
            }
        }

        public void TripstripToTriangles(ref List<GeomTriangle> triangles)
        {
            if (triangles == null) return;

            for (int i = 0; i < NofVertices - 2; i++)
            {
                triangles.Add(
                    new GeomTriangle(
                           Vertex[i].X, Vertex[i].Y,
                           Vertex[i + 1].X, Vertex[i + 1].Y,
                           Vertex[i + 2].X, Vertex[i + 2].Y));
            }
        }

        public PointF CenterOfLargestTriangel
        {
            get
            {
                int pos = -1;
                for (int i = 0; i < NofVertices - 2; i++)
                {
                    pos = 0;
                    break;
                }
                if (pos != -1)
                {
                    double x = (Vertex[pos].X + Vertex[pos + 1].X + Vertex[pos + 2].X) / 3;
                    double y = (Vertex[pos].Y + Vertex[pos + 1].Y + Vertex[pos + 2].Y) / 3;

                    return new PointF((float)x, (float)y);
                }
                return new PointF(0, 0);
            }
        }
        public override string ToString()
		{
			string s = "Polygon with " + NofVertices + " vertices: ";
			
			for ( int i=0 ; i<NofVertices ; i++ ) {
				s += Vertex[i].ToString();
				if ( i!=NofVertices-1 )
					s += ",";
			}
			return s;
		}
	}

	internal class GeomPolygon
	{
		public int          NofContours;
		public bool[]       ContourIsHole;
		public GeomVertexList[] Contour;

		public GeomPolygon()
		{
		}

		// path should contain only polylines ( use Flatten )
		// furthermore the constructor assumes that all Subpathes of path except the first one are holes
        public GeomPolygon(GraphicsPath path)
        {
            NofContours = 0;
            foreach (byte b in path.PathTypes)
            {
                if ((b & ((byte)PathPointType.CloseSubpath)) != 0)
                    NofContours++;
            }

            ContourIsHole = new bool[NofContours];
            Contour = new GeomVertexList[NofContours];
            for (int i = 0; i < NofContours; i++)
                ContourIsHole[i] = (i == 0);

            int contourNr = 0;
            ArrayList contour = new ArrayList();
            for (int i = 0; i < path.PathPoints.Length; i++)
            {
                contour.Add(path.PathPoints[i]);
                if ((path.PathTypes[i] & ((byte)PathPointType.CloseSubpath)) != 0)
                {
                    PointF[] pointArray = (PointF[])contour.ToArray(typeof(PointF));
                    GeomVertexList vl = new GeomVertexList(pointArray);
                    Contour[contourNr++] = vl;
                    contour.Clear();
                }
            }
        }

        public GeomPolygon(IPolygon polygon, double generalizationDistance = 0)
        {
            if (polygon == null || polygon.RingCount == 0) return;
            NofContours = polygon.RingCount;

            ContourIsHole = new bool[NofContours];
            Contour = new GeomVertexList[NofContours];
            for (int i = 0; i < NofContours; i++)
                ContourIsHole[i] = (i != 0);

            for (int i = 0; i < polygon.RingCount; i++)
            {
                Contour[i] = new GeomVertexList(polygon[i]);
            }
        }

        public GeomPolygon(IEnvelope envelope)
        {
            Polygon polygon = new Polygon();
            Ring ring = new Ring();

            ring.AddPoint(new gView.Framework.Geometry.Point(envelope.minx, envelope.miny));
            ring.AddPoint(new gView.Framework.Geometry.Point(envelope.minx, envelope.maxy));
            ring.AddPoint(new gView.Framework.Geometry.Point(envelope.maxx, envelope.maxy));
            ring.AddPoint(new gView.Framework.Geometry.Point(envelope.maxx, envelope.miny));
            ring.AddPoint(new gView.Framework.Geometry.Point(envelope.minx, envelope.miny));

            polygon.AddRing(ring);

            NofContours = polygon.RingCount;

            ContourIsHole = new bool[NofContours];
            Contour = new GeomVertexList[NofContours];
            for (int i = 0; i < NofContours; i++)
                ContourIsHole[i] = (i != 0);

            for (int i = 0; i < polygon.RingCount; i++)
            {
                Contour[i] = new GeomVertexList(polygon[i]);
            }
        }
        
        /*
        public GeomPolygon(IPolygon poly)
        {

        }
         * */

        /*
		public static GeomPolygon FromFile( string filename, bool readHoleFlags )
		{
			return ClipWrapper.ReadPolygon( filename, readHoleFlags );
		}
		*/
        public void AddContour(GeomVertexList contour, bool contourIsHole)
		{
			bool[]       hole = new bool[NofContours+1];
			GeomVertexList[] cont = new GeomVertexList[NofContours+1];
			
			for ( int i=0 ; i<NofContours ; i++ ) {
				hole[i] = ContourIsHole[i];
				cont[i] = Contour[i];
			}
			hole[NofContours]   = contourIsHole;
			cont[NofContours++] = contour;
			
			ContourIsHole = hole;
			Contour       = cont;
		}

		public GraphicsPath ToGraphicsPath()
		{
			GraphicsPath path = new GraphicsPath();
			
			for ( int i=0 ; i<NofContours ; i++ ) {
				PointF[] points = Contour[i].ToPoints();
				if ( ContourIsHole[i] )
					Array.Reverse( points );
				path.AddPolygon( points );
			}
			return path;
		}

        public IPolygon ToPolygon()
        {
            Polygon polygon = new Polygon();

            for (int i = 0; i < NofContours; i++)
            {
                polygon.AddRing(Contour[i].ToRing());
            }
            return polygon;
        }

		public override string ToString()
		{
			string s = "Polygon with " + NofContours.ToString() + " contours." + "\r\n";
			for ( int i=0 ; i<NofContours ; i++ ) {
				if ( ContourIsHole[i] )
					s += "Hole: ";
				else
					s += "Contour: ";
				s += Contour[i].ToString();
			}
			return s;
		}

		public GeomTristrip ClipToTristrip( ClipOperation operation, GeomPolygon polygon )
		{
			return ClipWrapper.ClipToTristrip( operation, this, polygon );
		}

		public GeomPolygon Clip( ClipOperation operation, GeomPolygon polygon )
		{
			return ClipWrapper.Clip( operation, this, polygon );
		}

		public GeomTristrip ToTristrip()
		{
			return ClipWrapper.GeomPolygonToTristrip( this );
		}

        /*
		public void Save( string filename, bool writeHoleFlags )
		{
			ClipWrapper.SavePolygon( filename, writeHoleFlags, this );
		}
         * */
	}
	
	internal class GeomTristrip
	{
		public int          NofStrips;
		public GeomVertexList[] Strip;
	}
	
	internal class ClipWrapper
	{
		public static GeomTristrip GeomPolygonToTristrip( GeomPolygon polygon )
		{
			Clip_tristrip Clip_strip = new Clip_tristrip();
			Clip_polygon Clip_pol = ClipWrapper.GeomPolygonTo_Clip_polygon( polygon );

            try
            {
                Polygon2Tristrip(ref Clip_pol, ref Clip_strip);
                GeomTristrip tristrip = ClipWrapper.Clip_strip_ToTristrip(Clip_strip);

                return tristrip;
            }
            finally
            {
                ClipWrapper.Free_Clip_polygon(Clip_pol);
                ClipWrapper.FreeTristrip(ref Clip_strip);
            }
		}
        public static GeomTristrip PolygonToTristip(IPolygon polygon)
        {
            Clip_tristrip clip_strip = new Clip_tristrip();
            Clip_polygon clip_pol = ClipWrapper.PolygonTo_Clip_polygon(polygon);

            try
            {
                Polygon2Tristrip(ref clip_pol, ref clip_strip);
                GeomTristrip tristrip = ClipWrapper.Clip_strip_ToTristrip(clip_strip);

                return tristrip;
            }
            finally
            {
                ClipWrapper.Free_Clip_polygon(clip_pol);
                ClipWrapper.FreeTristrip(ref clip_strip);
            }
        }

		public static GeomTristrip ClipToTristrip( ClipOperation operation, GeomPolygon subject_polygon, GeomPolygon clip_polygon )
		{
			Clip_tristrip Clip_strip = new Clip_tristrip();
			Clip_polygon Clip_subject_polygon = ClipWrapper.GeomPolygonTo_Clip_polygon( subject_polygon );
			Clip_polygon Clip_clip_polygon    = ClipWrapper.GeomPolygonTo_Clip_polygon( clip_polygon );

            try
            {
                ClipTristrip(ref Clip_subject_polygon, ref Clip_clip_polygon, operation, ref Clip_strip);
                GeomTristrip tristrip = ClipWrapper.Clip_strip_ToTristrip(Clip_strip);

                return tristrip;
            }
            finally
            {
                ClipWrapper.Free_Clip_polygon(Clip_subject_polygon);
                ClipWrapper.Free_Clip_polygon(Clip_clip_polygon);
                ClipWrapper.FreeTristrip(ref Clip_strip);
            }

			
		}

		public static GeomPolygon Clip( ClipOperation operation, GeomPolygon subject_polygon, GeomPolygon clip_polygon )
		{
			Clip_polygon Clip_polygon         = new Clip_polygon();
			Clip_polygon Clip_subject_polygon = ClipWrapper.GeomPolygonTo_Clip_polygon( subject_polygon );
			Clip_polygon Clip_clip_polygon    = ClipWrapper.GeomPolygonTo_Clip_polygon( clip_polygon );

            try
            {
                ClipPolygon(ref Clip_subject_polygon, ref Clip_clip_polygon, operation, ref Clip_polygon);
                GeomPolygon polygon = ClipWrapper.Clip_polygon_ToGeomPolygon(Clip_polygon);

                return polygon;
            }
            finally
            {
                ClipWrapper.Free_Clip_polygon(Clip_subject_polygon);
                ClipWrapper.Free_Clip_polygon(Clip_clip_polygon);
                ClipWrapper.FreePolygon(ref Clip_polygon);
            }
		}

        public static IPolygon Clip(ClipOperation operation, IPolygon subject_polygon, IPolygon clip_polygon)
        {
            Clip_polygon Clip_polygon = new Clip_polygon();
            Clip_polygon Clip_subject_polygon = ClipWrapper.PolygonTo_Clip_polygon(subject_polygon);
            Clip_polygon Clip_clip_polygon = ClipWrapper.PolygonTo_Clip_polygon(clip_polygon);

            try
            {
                ClipPolygon(ref Clip_subject_polygon, ref Clip_clip_polygon, operation, ref Clip_polygon);
                IPolygon polygon = ClipWrapper.Clip_polygon_ToPolygon(Clip_polygon);

                return polygon;
            }
            finally
            {
                ClipWrapper.Free_Clip_polygon(Clip_subject_polygon);
                ClipWrapper.Free_Clip_polygon(Clip_clip_polygon);
                ClipWrapper.FreePolygon(ref Clip_polygon);
            }
        }

        public static IPolygon Union(List<IPolygon> polygons)
        {
            if (polygons.Count == 0) return null;
            if (polygons.Count == 1) return polygons[0];

            /*
            if (polygons.Count > 100)
            {
                return polygons[0];
            }
            */

            Clip_polygon union_polygon = new Clip_polygon();
            Clip_polygon polygon1 = ClipWrapper.PolygonTo_Clip_polygon(polygons[0]);
            try
            {
                for (int i = 1; i < polygons.Count; i++)
                {
                    union_polygon = new Clip_polygon();
                    Clip_polygon polygon2 = ClipWrapper.PolygonTo_Clip_polygon(polygons[i]);

                    ClipPolygon(ref polygon1, ref polygon2, ClipOperation.Union, ref union_polygon);
                    if (i == 1)
                        ClipWrapper.Free_Clip_polygon(polygon1);
                    else
                        ClipWrapper.FreePolygon(ref polygon1);
                    ClipWrapper.Free_Clip_polygon(polygon2);

                    polygon1 = union_polygon;
                }
                IPolygon polygon = ClipWrapper.Clip_polygon_ToPolygon(union_polygon);

                return polygon;
            }
            finally
            {
                ClipWrapper.FreePolygon(ref union_polygon);
            }
        }

        public static IPolygon BufferPath(IPath path, double distance)
        {
            Clip_vertex_list vtx_lst = PathTo_Clip_vertex_list(path);
            Clip_polygon buffer_polygon = new Clip_polygon();

            try
            {
                BufferVertextList(ref vtx_lst, distance, BufferOperation.Buffer_LINES, ref buffer_polygon);
                IPolygon polygon = ClipWrapper.Clip_polygon_ToPolygon(buffer_polygon);

                return polygon;
            }
            finally
            {
                ClipWrapper.Free_Clip_polygon(buffer_polygon);
                ClipWrapper.Free_Clip_vertex_list(vtx_lst);
            }
        }
        /*
		public static void SavePolygon( string filename, bool writeHoleFlags, GeomPolygon polygon )
		{
			Clip_polygon Clip_polygon = ClipWrapper.PolygonTo_Clip_polygon( polygon );

			IntPtr fp = fopen( filename, "wb" );
			Clip_write_polygon( fp, writeHoleFlags?((int)1):((int)0), ref Clip_polygon );
			fclose( fp );

			ClipWrapper.Free_Clip_polygon( Clip_polygon );
		}

		public static GeomPolygon ReadPolygon( string filename, bool readHoleFlags )
		{
			Clip_polygon Clip_polygon = new Clip_polygon();

			IntPtr fp = fopen( filename, "rb" );
			Clip_read_polygon( fp, readHoleFlags?((int)1):((int)0), ref Clip_polygon );
			GeomPolygon polygon = Clip_polygon_ToPolygon( Clip_polygon );
			FreePolygon( ref Clip_polygon );
			fclose( fp );
			
			return polygon;
		}
         * */

        private static Clip_polygon GeomPolygonTo_Clip_polygon(GeomPolygon polygon)
		{
			Clip_polygon  Clip_pol   = new Clip_polygon();
			Clip_pol.num_contours = polygon.NofContours;

			int[] hole = new int[polygon.NofContours];
			for ( int i=0 ; i<polygon.NofContours ; i++ )
				hole[i] = (polygon.ContourIsHole[i] ? 1 : 0 );
			Clip_pol.hole         = Marshal.AllocCoTaskMem( polygon.NofContours * Marshal.SizeOf(hole[0]) );
			Marshal.Copy( hole, 0, Clip_pol.hole, polygon.NofContours );
			
			Clip_pol.contour = Marshal.AllocCoTaskMem( polygon.NofContours * Marshal.SizeOf( new Clip_vertex_list() ) );
			IntPtr ptr = Clip_pol.contour;
			for ( int i=0 ; i<polygon.NofContours ; i++ ) {
				Clip_vertex_list Clip_vtx_list = new Clip_vertex_list();
				Clip_vtx_list.num_vertices    = polygon.Contour[i].NofVertices;
				Clip_vtx_list.vertex          = Marshal.AllocCoTaskMem( polygon.Contour[i].NofVertices * Marshal.SizeOf(new Clip_vertex()) );
				IntPtr ptr2 = Clip_vtx_list.vertex;
				for ( int j=0 ; j<polygon.Contour[i].NofVertices ; j++ ) {
					Clip_vertex Clip_vtx        = new Clip_vertex();
					Clip_vtx.x                 = polygon.Contour[i].Vertex[j].X;
					Clip_vtx.y                 = polygon.Contour[i].Vertex[j].Y;
					Marshal.StructureToPtr( Clip_vtx, ptr2, false );
                    ptr2 = IntPtrPlus(ptr2, Marshal.SizeOf(Clip_vtx)); // (IntPtr)(((int)ptr2) + Marshal.SizeOf(Clip_vtx));
				}
				Marshal.StructureToPtr( Clip_vtx_list, ptr, false );
                ptr = IntPtrPlus(ptr, Marshal.SizeOf(Clip_vtx_list)); //(IntPtr)(((int)ptr) + Marshal.SizeOf(Clip_vtx_list));
			}

			return Clip_pol;
		}

        private static Clip_polygon PolygonTo_Clip_polygon(IPolygon polygon)
        {
            if (polygon == null || polygon.RingCount == 0) return new Clip_polygon();

            int RingCount = polygon.RingCount;
            Clip_polygon Clip_pol = new Clip_polygon();
            Clip_pol.num_contours = RingCount;

            int[] hole = new int[RingCount];
            for (int i = 0; i < RingCount; i++)
                hole[i] = ((polygon[i] is IHole) ? 1 : 0);
            Clip_pol.hole = Marshal.AllocCoTaskMem(RingCount * Marshal.SizeOf(hole[0]));
            Marshal.Copy(hole, 0, Clip_pol.hole, hole.Length);

            Clip_pol.contour = Marshal.AllocCoTaskMem(RingCount * Marshal.SizeOf(new Clip_vertex_list()));
            IntPtr ptr = Clip_pol.contour;
            for (int i = 0; i < RingCount; i++)
            {
                Clip_vertex_list Clip_vtx_lst = new Clip_vertex_list();
                
                IRing ring=polygon[i];
                int PointCount=ring.PointCount;

                if (ring[0].X != ring[PointCount - 1].X ||
                    ring[0].Y != ring[PointCount - 1].Y)
                {
                    PointCount += 1;
                }

                Clip_vtx_lst.num_vertices = PointCount;
                Clip_vtx_lst.vertex = Marshal.AllocCoTaskMem(PointCount * Marshal.SizeOf(new Clip_vertex()));
                IntPtr ptr2 = Clip_vtx_lst.vertex;
                for (int j = 0; j < PointCount; j++)
                {
                    IPoint point = ((j < ring.PointCount) ? ring[j] : ring[0]);
                    Clip_vertex Clip_vtx = new Clip_vertex();
                    Clip_vtx.x = point.X;
                    Clip_vtx.y = point.Y;
                    Marshal.StructureToPtr(Clip_vtx, ptr2, false);
                    ptr2 = IntPtrPlus(ptr2, Marshal.SizeOf(Clip_vtx));// (IntPtr)(((int)ptr2) + Marshal.SizeOf(Clip_vtx));
                }

                Marshal.StructureToPtr(Clip_vtx_lst, ptr, false);
                ptr = IntPtrPlus(ptr, Marshal.SizeOf(Clip_vtx_lst)); //(IntPtr)(((int)ptr) + Marshal.SizeOf(Clip_vtx_lst));
            }

            return Clip_pol;
        }

        private static Clip_vertex_list PathTo_Clip_vertex_list(IPath path)
        {
            if (path == null) return new Clip_vertex_list();

            Clip_vertex_list Clip_vtx_lst = new Clip_vertex_list();
            int PointCount = path.PointCount;

            if (path is IRing)
            {
                if (path[0].X != path[PointCount - 1].X ||
                    path[0].Y != path[PointCount - 1].Y)
                {
                    PointCount += 1;
                }
            }

            Clip_vtx_lst.num_vertices = PointCount;
            Clip_vtx_lst.vertex = Marshal.AllocCoTaskMem(PointCount * Marshal.SizeOf(new Clip_vertex()));

            IntPtr ptr2 = Clip_vtx_lst.vertex;
            for (int j = 0; j < PointCount; j++)
            {
                IPoint point = ((j < path.PointCount) ? path[j] : path[0]);
                Clip_vertex Clip_vtx = new Clip_vertex();
                Clip_vtx.x = point.X;
                Clip_vtx.y = point.Y;
                Marshal.StructureToPtr(Clip_vtx, ptr2, false);
                ptr2 = IntPtrPlus(ptr2, Marshal.SizeOf(Clip_vtx)); //(IntPtr)(((int)ptr2) + Marshal.SizeOf(Clip_vtx));
            }

            return Clip_vtx_lst;
        }
        private static GeomPolygon Clip_polygon_ToGeomPolygon(Clip_polygon Clip_polygon)
		{
			GeomPolygon polygon = new GeomPolygon();
			
			polygon.NofContours = Clip_polygon.num_contours;
            if (polygon.NofContours == 0) return new GeomPolygon();

			polygon.ContourIsHole = new bool[polygon.NofContours];
			polygon.Contour       = new GeomVertexList[polygon.NofContours];
			short[] holeShort = new short[polygon.NofContours];
			IntPtr ptr = Clip_polygon.hole;
			
            Marshal.Copy( Clip_polygon.hole, holeShort, 0, polygon.NofContours );
            
			for ( int i=0 ; i<polygon.NofContours ; i++ )
				polygon.ContourIsHole[i] = (holeShort[i]!=0);

			ptr = Clip_polygon.contour;
			for ( int i=0 ; i<polygon.NofContours ; i++ ) {
				Clip_vertex_list Clip_vtx_list = (Clip_vertex_list)Marshal.PtrToStructure( ptr, typeof(Clip_vertex_list) );
				polygon.Contour[i] = new GeomVertexList();
				polygon.Contour[i].NofVertices = Clip_vtx_list.num_vertices;
				polygon.Contour[i].Vertex      = new GeomVertex[polygon.Contour[i].NofVertices];
				IntPtr ptr2 = Clip_vtx_list.vertex;
				for ( int j=0 ; j<polygon.Contour[i].NofVertices ; j++ ) {
					Clip_vertex Clip_vtx = (Clip_vertex)Marshal.PtrToStructure( ptr2, typeof(Clip_vertex) );
					polygon.Contour[i].Vertex[j].X = Clip_vtx.x;
					polygon.Contour[i].Vertex[j].Y = Clip_vtx.y;

                    ptr2 = IntPtrPlus(ptr2, Marshal.SizeOf(Clip_vtx)); //(IntPtr)(((int)ptr2) + Marshal.SizeOf(Clip_vtx));
				}
                ptr = IntPtrPlus(ptr, Marshal.SizeOf(Clip_vtx_list)); //(IntPtr)(((int)ptr) + Marshal.SizeOf(Clip_vtx_list));
			}

			return polygon;
		}

        private static IPolygon Clip_polygon_ToPolygon(Clip_polygon Clip_polygon)
        {
            Polygon polygon = new Polygon();

            if (Clip_polygon.num_contours == 0) return new Polygon();

            short[] holeShort = new short[Clip_polygon.num_contours];
            Marshal.Copy(Clip_polygon.hole, holeShort, 0, Clip_polygon.num_contours);

            IntPtr ptr = Clip_polygon.contour;
            for (int i = 0; i < Clip_polygon.num_contours; i++)
            {
                Clip_vertex_list Clip_vtx_lst = (Clip_vertex_list)Marshal.PtrToStructure(ptr, typeof(Clip_vertex_list));
                IRing ring = ((holeShort[i] == 0) ? new Ring() : new Hole());

                IntPtr ptr2 = Clip_vtx_lst.vertex;
                for (int j = 0; j < Clip_vtx_lst.num_vertices; j++)
                {
                    Clip_vertex Clip_vtx = (Clip_vertex)Marshal.PtrToStructure(ptr2, typeof(Clip_vertex));
                    ring.AddPoint(new gView.Framework.Geometry.Point(
                        Clip_vtx.x, Clip_vtx.y));

                    ptr2 = IntPtrPlus(ptr2, Marshal.SizeOf(Clip_vtx)); //(IntPtr)(((int)ptr2) + Marshal.SizeOf(Clip_vtx));
                }
                polygon.AddRing(ring);
                ptr = IntPtrPlus(ptr, Marshal.SizeOf(Clip_vtx_lst)); //(IntPtr)(((int)ptr) + Marshal.SizeOf(Clip_vtx_lst));
            }
            return polygon;
        }
        private static GeomTristrip Clip_strip_ToTristrip(Clip_tristrip Clip_strip)
		{
			GeomTristrip tristrip  = new GeomTristrip();
			tristrip.NofStrips = Clip_strip.num_strips;
			tristrip.Strip     = new GeomVertexList[tristrip.NofStrips];
			IntPtr ptr = Clip_strip.strip;
			for ( int i=0 ; i<tristrip.NofStrips ; i++ ) {
				tristrip.Strip[i] = new GeomVertexList();
				Clip_vertex_list Clip_vtx_list = (Clip_vertex_list)Marshal.PtrToStructure( ptr, typeof(Clip_vertex_list) );
				tristrip.Strip[i].NofVertices = Clip_vtx_list.num_vertices;
				tristrip.Strip[i].Vertex      = new GeomVertex[tristrip.Strip[i].NofVertices];

				IntPtr ptr2 = Clip_vtx_list.vertex;
				for ( int j=0 ; j<tristrip.Strip[i].NofVertices ; j++ ) {
					Clip_vertex Clip_vtx = (Clip_vertex)Marshal.PtrToStructure( ptr2, typeof(Clip_vertex) );
					tristrip.Strip[i].Vertex[j].X = Clip_vtx.x;
					tristrip.Strip[i].Vertex[j].Y = Clip_vtx.y;

                    ptr2 = IntPtrPlus(ptr2, Marshal.SizeOf(Clip_vtx)); // (IntPtr)(((int)ptr2) + Marshal.SizeOf(Clip_vtx));
				}
                ptr = IntPtrPlus(ptr, Marshal.SizeOf(Clip_vtx_list));// (IntPtr)(((int)ptr) + Marshal.SizeOf(Clip_vtx_list));
			}

			return tristrip;
		}

		private static void Free_Clip_polygon( Clip_polygon Clip_pol )
		{
			Marshal.FreeCoTaskMem( Clip_pol.hole );
			IntPtr ptr = Clip_pol.contour;
			for ( int i=0 ; i<Clip_pol.num_contours ; i++ ) {
				Clip_vertex_list Clip_vtx_list = (Clip_vertex_list)Marshal.PtrToStructure( ptr, typeof(Clip_vertex_list) );
				Marshal.FreeCoTaskMem( Clip_vtx_list.vertex );
                ptr = IntPtrPlus(ptr, Marshal.SizeOf(Clip_vtx_list)); //(IntPtr)(((int)ptr) + Marshal.SizeOf(Clip_vtx_list));
			}
            Marshal.FreeCoTaskMem(Clip_pol.contour);
		}

        private static void Free_Clip_vertex_list(Clip_vertex_list Clip_vtx_list)
        {
            Marshal.FreeCoTaskMem(Clip_vtx_list.vertex);
        }

        private static IntPtr IntPtrPlus(IntPtr ptr, int plus)
        {
            if (IntPtr.Size == 8) // 64 Bit
                return (IntPtr)(ptr.ToInt64() + plus);
            return (IntPtr)(ptr.ToInt32() + plus);
        }

		[DllImport( "geom.dll" )]
		private static extern void Polygon2Tristrip(    [In]     ref Clip_polygon  polygon,
		                                                [In,Out] ref Clip_tristrip tristrip  );

		[DllImport( "geom.dll" )]
		private static extern void ClipPolygon(         [In]     ref Clip_polygon  subject_polygon,
		                                                [In]     ref Clip_polygon  clip_polygon,
                                                        [In]     ClipOperation set_operation,
		                                                [In,Out] ref Clip_polygon  result_polygon   );

		[DllImport( "geom.dll" )]
		private static extern void ClipTristrip(        [In]     ref Clip_polygon  subject_polygon,
		                                                [In]     ref Clip_polygon  clip_polygon,
                                                        [In]     ClipOperation set_operation,
		                                                [In,Out] ref Clip_tristrip result_tristrip  );

		[DllImport( "geom.dll" )]
		private static extern void FreeTristrip( [In] ref Clip_tristrip tristrip );

		[DllImport( "geom.dll" )]
		private static extern void FreePolygon( [In] ref Clip_polygon polygon );

        [DllImport("geom.dll") ]
        private static extern void BufferVertextList( [In]   ref Clip_vertex_list vtx_list,
                                                      [In]   double distance,
                                                      [In]   BufferOperation buffer_op,
                                                      [In, Out] ref Clip_polygon result_polygon   );  
                                                      

        /*
		[DllImport( "geom.dll" )]
		private static extern void Clip_read_polygon( [In] IntPtr fp, [In] int read_hole_flags, [In,Out] ref Clip_polygon polygon );

		[DllImport( "geom.dll" )]
		private static extern void Clip_write_polygon( [In] IntPtr fp, [In] int write_hole_flags, [In] ref Clip_polygon polygon );
        */
        /*
		[DllImport( "msvcr71.dll" )]
		private static extern IntPtr fopen( [In] string filename, [In] string mode );
		
		[DllImport( "msvcr71.dll" )]
		private static extern void fclose( [In] IntPtr fp );
		
		[DllImport( "msvcr71.dll" )]
		private static extern int fputc( [In] int c, [In] IntPtr fp );
        */
		enum Clip_op                                   /* Set operation type                */
		{
			Clip_DIFF  = 0,                             /* Difference                        */
			Clip_INT   = 1,                             /* Intersection                      */
			Clip_XOR   = 2,                             /* Exclusive or                      */
			Clip_UNION = 3                              /* Union                             */
		}
	
		[StructLayout(LayoutKind.Sequential)]
		private struct Clip_vertex                    /* Polygon vertex structure          */
		{ 
			public double              x;            /* Vertex x component                */
			public double              y;            /* vertex y component                */
		}
	
		[StructLayout(LayoutKind.Sequential)]
		private struct Clip_vertex_list               /* Vertex list structure             */
		{ 
			public int                 num_vertices; /* Number of vertices in list        */
			public IntPtr              vertex;       /* Vertex array pointer              */
		}
	
		[StructLayout(LayoutKind.Sequential)]
		private struct Clip_polygon                   /* Polygon set structure             */
		{ 
			public int                 num_contours; /* Number of contours in polygon     */
            public IntPtr              contour;      /* Contour array pointer             */
			public IntPtr              hole;         /* Hole / external contour flags     */
		}
	
		[StructLayout(LayoutKind.Sequential)]
		private struct Clip_tristrip                  /* Tristrip set structure            */
		{ 
			public int                 num_strips;   /* Number of tristrips               */
			public IntPtr              strip;        /* Tristrip array pointer            */
		}
	}
}