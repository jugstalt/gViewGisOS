using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.SpatialAlgorithms;
using gView.Framework.Topology;

namespace gView.Framework.GeoProcessing
{
    [gView.Framework.system.RegisterPlugIn("F08AD765-E0F9-471c-B729-ACA08A6668D2")]
    public class Voronoi : gView.Framework.GeoProcessing.ActivityBase.SimpleActivity
    {
        #region IActivity Member
        public override string CategoryName
        {
            get { return "Trianguation"; }
        }

        public override string DisplayName
        {
            get { return "Voronoi Graph"; }
        }

        public override List<IActivityData> Process()
        {
            IDatasetElement sElement = base.SourceDatasetElement;
            IFeatureClass sFc = base.SourceFeatureClass;

            ActivityBase.TargetDatasetElement tElement = base.TargetDatasetElement;
            IDataset tDs = base.TargetDataset;
            //IFeatureDatabase tDatabase = base.TargetFeatureDatabase;

            GeometryDef geomDef = new GeometryDef(
                geometryType.Polyline,
                null,
                false);

            IFeatureClass tFc = base.CreateTargetFeatureclass(geomDef, sFc.Fields);
            IFeatureDatabase tDatabase = FeatureDatabase(tFc);

            Report.featureMax = sFc.CountFeatures;
            Report.featurePos = 0;
            ReportProgess("Query Filter: " + SourceData.FilterClause);

            Nodes nodes = new Nodes();
            using (IFeatureCursor cursor = SourceData.GetFeatures(String.Empty))
            {
                if (cursor == null) return null;

                IFeature feature;
                while ((feature = cursor.NextFeature) != null)
                {
                    if (Report.featurePos++ % 100 == 0)
                        ReportProgess();

                    if (feature.Shape is IPoint)
                        nodes.Add((IPoint)feature.Shape);
                }
            }

            VoronoiGraph voronoi = new VoronoiGraph();
            voronoi.ProgressMessage += new VoronoiGraph.ProgressMessageEventHandler(voronoi_ProgressMessage);
            voronoi.Progress += new VoronoiGraph.ProgressEventHandler(voronoi_Progress);
            voronoi.Calc(nodes);
            List<IPoint> vertices = voronoi.Nodes;
            Edges edges = voronoi.Edges;

            ReportProgess("Write Lines");
            Report.featurePos = 0;
            List<IFeature> features = new List<IFeature>();
            foreach (Edge edge in edges)
            {
                Polyline pLine = new Polyline();
                Path path = new Path();
                path.AddPoint(vertices[edge.p1]);
                path.AddPoint(vertices[edge.p2]);
                pLine.AddPath(path);

                Feature feature = new Feature();
                feature.Shape = pLine;

                features.Add(feature);
                Report.featurePos++;
                if (features.Count >= 100)
                {
                    if (!tDatabase.Insert(tFc, features))
                        throw new Exception(tDatabase.lastErrorMsg);
                    features.Clear();
                    ReportProgess();
                }
            }

            if (features.Count > 0)
            {
                ReportProgess();
                if (!tDatabase.Insert(tFc, features))
                    throw new Exception(tDatabase.lastErrorMsg);
            }

            ReportProgess("Flush Features");
            base.FlushFeatureClass(tFc);

            return base.ToProcessResult(tFc);
        }

        void voronoi_ProgressMessage(string msg)
        {
            ReportProgess(msg);
        }

        void voronoi_Progress(int pos, int max)
        {
            Report.featureMax = max;
            Report.featurePos = pos;
            ReportProgess();
        }
        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("FF2854AB-866E-4395-941B-BEFB2203E611")]
    public class Delaunay : gView.Framework.GeoProcessing.ActivityBase.SimpleActivity
    {
        #region IActivity Member
        public override string CategoryName
        {
            get { return "Trianguation"; }
        }

        public override string DisplayName
        {
            get { return "Delaunay Graph"; }
        }

        public override List<IActivityData> Process()
        {
            IDatasetElement sElement = base.SourceDatasetElement;
            IFeatureClass sFc = base.SourceFeatureClass;

            ActivityBase.TargetDatasetElement tElement = base.TargetDatasetElement;
            IDataset tDs = base.TargetDataset;
            //IFeatureDatabase tDatabase = base.TargetFeatureDatabase;

            GeometryDef geomDef = new GeometryDef(
                geometryType.Polyline,
                null,
                false);

            IFeatureClass tFc = base.CreateTargetFeatureclass(geomDef, sFc.Fields);
            IFeatureDatabase tDatabase = FeatureDatabase(tFc);

            Report.featureMax = sFc.CountFeatures;
            Report.featurePos = 0;
            ReportProgess("Query Filter: " + SourceData.FilterClause);

            Nodes nodes = new Nodes();
            using (IFeatureCursor cursor = SourceData.GetFeatures(String.Empty))
            {
                if (cursor == null) return null;

                IFeature feature;
                while ((feature = cursor.NextFeature) != null)
                {
                    if (Report.featurePos++ % 100 == 0)
                        ReportProgess();
                    
                    if (feature.Shape is IPoint)
                        nodes.Add((IPoint)feature.Shape);
                }
            }

            DelaunayTriangulation triangulation = new DelaunayTriangulation();
            triangulation.Progress += new DelaunayTriangulation.ProgressEventHandler(triangulation_Progress);
            ReportProgess("Calculate Triangles");
            Triangles triangles = triangulation.Triangulate(nodes);
            ReportProgess("Extract Edges");
            Edges edges = triangulation.TriangleEdges(triangles);

            Report.featurePos = 0;
            List<IFeature> features = new List<IFeature>();
            foreach (Edge edge in edges)
            {
                Polyline pLine = new Polyline();
                Path path = new Path();
                path.AddPoint(nodes[edge.p1]);
                path.AddPoint(nodes[edge.p2]);
                pLine.AddPath(path);

                Feature feature = new Feature();
                feature.Shape = pLine;

                Report.featurePos++;
                features.Add(feature);
                if (features.Count >= 100)
                {
                    if (!tDatabase.Insert(tFc, features))
                        throw new Exception(tDatabase.lastErrorMsg);
                    features.Clear();
                    ReportProgess();
                }
            }

            if (features.Count > 0)
            {
                ReportProgess();
                if (!tDatabase.Insert(tFc, features))
                    throw new Exception(tDatabase.lastErrorMsg);
            }

            ReportProgess("Flush Features");
            base.FlushFeatureClass(tFc);

            return base.ToProcessResult(tFc);
        }

        void triangulation_Progress(int pos, int max)
        {
            Report.featureMax = max;
            Report.featurePos = pos;
            ReportProgess();
        }
        #endregion
    }
}
