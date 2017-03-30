using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.GeoProcessing;
using gView.Framework.UI;
using gView.Framework.system;
using System.Reflection;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.SpatialAlgorithms;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;

namespace gView.Framework.GeoProcessing.ActivityBase
{
    public abstract class SimpleActivity : IActivity, IDirtyEvent
    {
        private string _errMsg = String.Empty;
        private ActivityFeatureData _sourceData = new ActivityFeatureData("Source Data");
        private ActivityFeatureData _targetData = new ActivityFeatureData("Destination Data");
        private CancelTracker _cancelTracker;

        public SimpleActivity()
        {
            _sourceData.DirtyEvent += new EventHandler(_sourceData_DirtyEvent);
            _targetData.DirtyEvent += new EventHandler(_targetData_DirtyEvent);
            _cancelTracker = new CancelTracker();
        }

        void _targetData_DirtyEvent(object sender, EventArgs e)
        {
            if (DirtyEvent != null)
                DirtyEvent(this, e);
        }

        void _sourceData_DirtyEvent(object sender, EventArgs e)
        {
            if (DirtyEvent != null)
                DirtyEvent(this, e);
        }

        #region IActivity Member

        public List<IActivityData> Sources
        {
            get
            {
                List<IActivityData> list = new List<IActivityData>();
                list.Add(_sourceData);

                return list;
            }
        }

        public List<IActivityData> Targets
        {
            get
            {
                List<IActivityData> list = new List<IActivityData>();
                list.Add(_targetData);

                return list;
            }
        }

        public abstract string DisplayName
        {
            get ;
        }

        public abstract string CategoryName
        {
            get ;
        }

        public abstract List<IActivityData> Process();

        #endregion

        #region IErrorMessage Member

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        #endregion

        #region IDirtyEvent Member

        public event EventHandler DirtyEvent = null;

        #endregion

        #region IProgressReporter Member

        virtual public event ProgressReporterEvent ReportProgress;

        public ICancelTracker CancelTracker
        {
            get { return _cancelTracker; }
        }

        #endregion

        #region Helper
        protected IActivityData SourceData
        {
            get { return _sourceData; }
        }
        protected IDatasetElement SourceDatasetElement
        {
            get
            {
                if (_sourceData.Data == null)
                    throw new ArgumentException("No Sourcedata selected...");

                return _sourceData.Data;
            }
        }
        protected IFeatureClass SourceFeatureClass
        {
            get
            {
                IDatasetElement sElement = this.SourceDatasetElement;
                if (!(sElement.Class is IFeatureClass))
                    throw new ArgumentException("No Sourcefeatureclass available...");

                return (IFeatureClass)sElement.Class;
            }
        }
        protected TargetDatasetElement TargetDatasetElement
        {
            get
            {
                if (!(_targetData.Data is TargetDatasetElement))
                    throw new ArgumentException("No Targetdata selected...");

                return _targetData.Data as TargetDatasetElement;
            }
        }
        protected IDataset TargetDataset
        {
            get
            {
                TargetDatasetElement tElement = this.TargetDatasetElement;

                if (tElement.Class == null)
                    throw new ArgumentException("No Targetfeatureclass...");
                if (tElement.Class.Dataset == null)
                    throw new ArgumentException("No Targetdataset...");

                return tElement.Class.Dataset;
            }
        }
        protected IFeatureDatabase TargetFeatureDatabase
        {
            get
            {
                TargetDatasetElement tElement = this.TargetDatasetElement;

                if (tElement.Class == null)
                    throw new ArgumentException("No Targetfeatureclass...");
                if (tElement.Class.Dataset == null)
                    throw new ArgumentException("No Targetdataset...");
                if (!(tElement.Class.Dataset.Database is IFeatureDatabase))
                    throw new ArgumentException("No Targetdatabase...");

                return (IFeatureDatabase)tElement.Class.Dataset.Database;
            }
        }
        protected IFeatureDatabase FeatureDatabase(IFeatureClass fc)
        {
            if (fc == null)
                throw new ArgumentException("No Targetfeatureclass...");
            if (fc.Dataset == null)
                throw new ArgumentException("No Targetdataset...");
            if (!(fc.Dataset.Database is IFeatureDatabase))
                throw new ArgumentException("No Targetdatabase...");

            return (IFeatureDatabase)fc.Dataset.Database;
        }

        protected IFeatureClass CreateTargetFeatureclass(IGeometryDef geomDef, IFields fields)
        {
            IFeatureClass sFc = SourceFeatureClass;
            BinaryTreeDef sDef = null;
            if (sFc.Dataset != null && sFc.Dataset.Database is AccessFDB)
            {
                AccessFDB sFdb = (AccessFDB)sFc.Dataset.Database;
                sDef = sFdb.BinaryTreeDef(sFc.Name);
            }
            
            TargetDatasetElement e = this.TargetDatasetElement;
            IDataset ds = this.TargetDataset;
            IFeatureDatabase db = this.TargetFeatureDatabase;

            if (db.CreateFeatureClass(ds.DatasetName, e.Title, geomDef, fields) == -1)
                throw new Exception("Can't create target featureclass:\n" + db.lastErrorMsg);

            IDatasetElement element = ds[e.Title];
            if(element==null || !(element.Class is IFeatureClass))
                throw new Exception("Can't open created featureclass");

            IFeatureClass tFc = element.Class as IFeatureClass;
            if (db is AccessFDB)
            {
                int maxAllowedLevel = ((db is SqlFDB) ? 62 : 30);
                if (sDef == null)
                {
                    IEnvelope sEnv = sFc.Envelope;
                    sDef = (sEnv != null) ? new BinaryTreeDef(sFc.Envelope, 10, 200, 0.55) : new BinaryTreeDef(new Envelope());
                }
                if (sDef.Bounds != null &&
                    sDef.SpatialReference != null &&
                    !sDef.SpatialReference.Equals(tFc.SpatialReference))
                {
                    if (!sDef.ProjectTo(tFc.SpatialReference))
                        throw new Exception("Can't project SpatialIndex Boundaries...");
                }
                ((AccessFDB)db).SetSpatialIndexBounds(tFc.Name, "BinaryTree2", sDef.Bounds, sDef.SplitRatio, sDef.MaxPerNode, Math.Min(sDef.MaxLevel, maxAllowedLevel));
                ((AccessFDB)db).SetFeatureclassExtent(tFc.Name, sDef.Bounds);
            }

            return tFc;
        }
        protected bool FlushFeatureClass(IFeatureClass fc)
        {
            IFeatureDatabase db = (IFeatureDatabase)fc.Dataset.Database;

            bool ret = true;
            if (db is IFileFeatureDatabase)
            {
                ret = ((IFileFeatureDatabase)db).Flush(fc);
            }
            else if (db is AccessFDB)
            {
                ret = ((AccessFDB)db).CalculateExtent(fc);
            }
            return ret;
        }

        protected List<IActivityData> ToProcessResult(IFeatureClass fc)
        {
            ActivityFeatureData aData = new ActivityFeatureData("Result");
            aData.Data = new DatasetElement(fc);

            List<IActivityData> list = new List<IActivityData>();
            list.Add(aData);

            return list;
        }

        private ProgressReport _report = new ProgressReport();
        protected ProgressReport Report
        {
            get { return _report; }
        }
        protected void ReportProgess()
        {
            ReportProgess(String.Empty);
        }
        protected void ReportProgess(string msg)
        {
            if (!String.IsNullOrEmpty(msg))
                _report.Message = msg;

            if (ReportProgress != null)
                ReportProgress(_report);
        }
        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("FC814EC6-3E09-44e6-9E8E-3286FAD26B8A")]
    public class Merger : SimpleActivity, IPropertyPage
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        private IField _mergeField = null;
        private object [] _values = null;

        public Merger() : base()
        {
        }

        #region Properties
        public IField MergeField
        {
            get { return _mergeField; }
            set { _mergeField = value; }
        }
        public object[] Values
        {
            get { return _values; }
            set { _values = value; }
        }
        #endregion

        #region IActivity Member

        override public string DisplayName
        {
            get { return "Merge Features"; }
        }

        override public string CategoryName
        {
            get { return "Base"; }
        }

        override public List<IActivityData> Process()
        {
            #region Where Clauses
            List<string> whereClauses = new List<string>();
            if (_mergeField == null)
            {
                whereClauses.Add(String.Empty);
            }
            else
            {
                foreach (object val in _values)
                {
                    if (val == null) continue;
                    switch (_mergeField.type)
                    {
                        case FieldType.smallinteger:
                        case FieldType.integer:
                        case FieldType.biginteger:
                            whereClauses.Add(_mergeField.name + "=" + val.ToString());
                            break;
                        case FieldType.Float:
                        case FieldType.Double:
                            whereClauses.Add(_mergeField.name + "=" + Convert.ToDouble(val).ToString(_nhi));
                            break;
                        case FieldType.boolean:
                            whereClauses.Add(_mergeField.name + "=" + val.ToString());
                            break;
                        case FieldType.String:
                            whereClauses.Add(_mergeField.name + "='" + val.ToString() + "'");
                            break;
                        default:
                            throw new Exception("Can't merge this fieldtype: " + _mergeField.type.ToString());
                    }
                }
            }
            #endregion

            IDatasetElement sElement = base.SourceDatasetElement;
            IFeatureClass sFc = base.SourceFeatureClass;

            TargetDatasetElement tElement = base.TargetDatasetElement;
            IDataset tDs = base.TargetDataset;
            //IFeatureDatabase tDatabase = base.TargetFeatureDatabase;

            Fields fields = new Fields();
            if (_mergeField != null)
                fields.Add(_mergeField);
            IFeatureClass tFc = base.CreateTargetFeatureclass(sFc, fields);
            IFeatureDatabase tDatabase = FeatureDatabase(tFc);

            Report.featureMax = sFc.CountFeatures;
            Report.featurePos = 0;

            bool isPolygonFc = (sFc.GeometryType == geometryType.Polygon);
            foreach (string whereClause in whereClauses)
            {

                ReportProgess("Query Filter: " + SourceData.FilterClause + (String.IsNullOrEmpty(whereClause) ? " AND " + whereClause : String.Empty));

                Feature mergeFeature = null; ;
                bool attributeAdded = false;
                using (IFeatureCursor cursor = SourceData.GetFeatures(whereClause))
                {
                    IFeature feature;

                    List<IPolygon> polygons = new List<IPolygon>();
                    while ((feature = cursor.NextFeature) != null)
                    {
                        if (mergeFeature == null)
                            mergeFeature = new Feature();

                        Report.featurePos++;
                        if (!CancelTracker.Continue) break;

                        if (_mergeField != null && attributeAdded == false)
                        {
                            mergeFeature.Fields.Add(new FieldValue(_mergeField.name, feature[_mergeField.name]));
                            attributeAdded = true;
                        }

                        if (isPolygonFc)
                        {
                            if (feature.Shape != null)
                            {
                                if (!(feature.Shape is IPolygon))
                                {
                                    throw new Exception("Wrong argument type :" + feature.Shape.GeometryType.ToString());
                                }
                                polygons.Add(feature.Shape as IPolygon);
                            }
                        }
                        else
                        {
                            if (mergeFeature.Shape == null)
                            {
                                mergeFeature.Shape = feature.Shape;
                            }
                            else
                            {
                                mergeFeature.Shape = Algorithm.Merge(mergeFeature.Shape, feature.Shape, false);
                            }
                        }
                        ReportProgess();
                    }
                    if (isPolygonFc && mergeFeature != null)
                    {
                        ProgressReporterEvent r = new ProgressReporterEvent(MergePolygonReport); 
                        mergeFeature.Shape = Algorithm.FastMergePolygon(polygons, CancelTracker, r);
                    }
                            
                }
                if (mergeFeature != null)
                {
                    if (!tDatabase.Insert(tFc, mergeFeature))
                        throw new Exception(tDatabase.lastErrorMsg);
                }
                if (!CancelTracker.Continue) break;
            }

            ReportProgess("Flush Features");
            base.FlushFeatureClass(tFc);

            return base.ToProcessResult(tFc);
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.GeoProcessing.UI.dll");

            IPlugInParameter p = uiAssembly.CreateInstance("gView.Framework.GeoProcessing.ActivityBase.MergeFeaturesControl") as IPlugInParameter;
            if (p != null)
            {
                p.Parameter = this;
                return p;
            }
            return null;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion

        public override event ProgressReporterEvent ReportProgress;
        protected void MergePolygonReport(ProgressReport report)
        {
            if (ReportProgress != null)
                ReportProgress(report);
        }
    }

    [gView.Framework.system.RegisterPlugIn("718BDF02-3CC2-46a2-B1D3-886B85A66A89")]
    public class Dissolve : SimpleActivity
    {
        override public string DisplayName
        {
            get { return "Dissolve Features"; }
        }
        public override string CategoryName
        {
            get { return "Base"; }
        }

        public override List<IActivityData> Process()
        {
            IDatasetElement sElement = base.SourceDatasetElement;
            IFeatureClass sFc = base.SourceFeatureClass;

            TargetDatasetElement tElement = base.TargetDatasetElement;
            IDataset tDs = base.TargetDataset;
            //IFeatureDatabase tDatabase = base.TargetFeatureDatabase;

            IFeatureClass tFc = base.CreateTargetFeatureclass(sFc, sFc.Fields);
            IFeatureDatabase tDatabase = FeatureDatabase(tFc);

            Report.featureMax = sFc.CountFeatures;
            Report.featurePos = 0;
            ReportProgess("Query Filter: " + SourceData.FilterClause);

            using (IFeatureCursor cursor = SourceData.GetFeatures(String.Empty))
            {
                IFeature feature;
                List<IFeature> features = new List<IFeature>();

                ReportProgess("Read/Write Features...");
                while ((feature = cursor.NextFeature) != null)
                {
                    if (feature.Shape == null) continue;

                    List<IGeometry> geometries = new List<IGeometry>();
                    #region Dissolve
                    if (feature.Shape is IPolygon)
                    {
                        foreach (IPolygon polygon in Algorithm.SplitPolygonToDonutsAndPolygons((IPolygon)feature.Shape))
                        {
                            geometries.Add(polygon);
                        }
                    }
                    else if (feature.Shape is IPolyline)
                    {
                        foreach (IPath path in Algorithm.GeometryPaths((IPolyline)feature.Shape))
                        {
                            Polyline pLine = new Polyline();
                            pLine.AddPath(path);
                            geometries.Add(pLine);
                        }
                    }
                    else if (feature.Shape is IMultiPoint)
                    {
                        for (int i = 0; i < ((IMultiPoint)feature.Shape).PointCount; i++)
                        {
                            IPoint p = ((IMultiPoint)feature.Shape)[i];
                            if (p != null) geometries.Add(p);
                        }
                    }
                    #endregion

                    if (geometries.Count > 0)
                    {
                        foreach (IGeometry geometry in geometries)
                        {
                            Feature f = new Feature(feature);
                            f.Shape = geometry;
                            if(!tDatabase.Insert(tFc,f))
                                throw new Exception(tDatabase.lastErrorMsg);
                        }
                    }
                }
            }

            ReportProgess("Flush Features");
            base.FlushFeatureClass(tFc);

            return base.ToProcessResult(tFc);
        }
    }

    [gView.Framework.system.RegisterPlugIn("62B9E36C-BA60-42A4-8B3E-E7156A018A25")]
    public class Filter : SimpleActivity
    {
        public Filter()
            : base()
        {
        }

        #region IActivity Member

        override public string DisplayName
        {
            get { return "Filter Features"; }
        }

        override public string CategoryName
        {
            get { return "Base"; }
        }

        override public List<IActivityData> Process()
        {
            IDatasetElement sElement = base.SourceDatasetElement;
            IFeatureClass sFc = base.SourceFeatureClass;

            TargetDatasetElement tElement = base.TargetDatasetElement;
            IDataset tDs = base.TargetDataset;
            //IFeatureDatabase tDatabase = base.TargetFeatureDatabase;

            IFeatureClass tFc = base.CreateTargetFeatureclass(sFc, sFc.Fields);
            IFeatureDatabase tDatabase = FeatureDatabase(tFc);

            Report.featureMax = sFc.CountFeatures;
            Report.featurePos = 0;
            ReportProgess("Query Filter: " + SourceData.FilterClause);

            using (IFeatureCursor cursor = SourceData.GetFeatures(String.Empty))
            {
                IFeature feature;
                List<IFeature> features = new List<IFeature>();

                ReportProgess("Read/Write Features...");
                while ((feature = cursor.NextFeature) != null)
                {
                    features.Add(feature);

                    Report.featurePos++;
                    if (!CancelTracker.Continue) break;

                    if (features.Count > 100)
                    {
                        ReportProgess();
                        if (!tDatabase.Insert(tFc, features))
                            throw new Exception(tDatabase.lastErrorMsg);
                        features.Clear();
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
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("6C053A99-6A9E-4BCE-9E5E-A7F46495347D")]
    public class Buffer : SimpleActivity, IPropertyPage
    {
        private double _bufferDistance = 30;

        public Buffer()
            : base()
        {
        }

        public double BufferDistance
        {
            get { return _bufferDistance; }
            set { _bufferDistance = value; }
        }

        #region IActivity Member

        override public string DisplayName
        {
            get { return "Buffer Features"; }
        }

        override public string CategoryName
        {
            get { return "Base"; }
        }

        override public List<IActivityData> Process()
        {
            IDatasetElement sElement = base.SourceDatasetElement;
            IFeatureClass sFc = base.SourceFeatureClass;

            TargetDatasetElement tElement = base.TargetDatasetElement;
            IDataset tDs = base.TargetDataset;
            //IFeatureDatabase tDatabase = base.TargetFeatureDatabase;

            GeometryDef geomDef = new GeometryDef(
                geometryType.Polygon,
                null,
                false);

            IFeatureClass tFc = base.CreateTargetFeatureclass(geomDef, sFc.Fields);
            IFeatureDatabase tDatabase = FeatureDatabase(tFc);

            Report.featureMax = sFc.CountFeatures;
            Report.featurePos = 0;
            ReportProgess("Query Filter: " + SourceData.FilterClause);

            using (IFeatureCursor cursor = SourceData.GetFeatures(String.Empty))
            {
                IFeature feature;
                List<IFeature> features = new List<IFeature>();

                ReportProgess("Read/Write Features...");
                while ((feature = cursor.NextFeature) != null)
                {
                    if (feature.Shape is ITopologicalOperation)
                        feature.Shape = ((ITopologicalOperation)feature.Shape).Buffer(_bufferDistance);
                    else
                        continue;

                    if (feature.Shape == null) continue;

                    features.Add(feature);

                    Report.featurePos++;
                    ReportProgess();
                    if (!CancelTracker.Continue) break;

                    if (features.Count > 100)
                    { 
                        if (!tDatabase.Insert(tFc, features))
                            throw new Exception(tDatabase.lastErrorMsg);
                        features.Clear();
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
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.GeoProcessing.UI.dll");

            IPlugInParameter p = uiAssembly.CreateInstance("gView.Framework.GeoProcessing.ActivityBase.BufferControl") as IPlugInParameter;
            if (p != null)
            {
                p.Parameter = this;
                return p;
            }
            return null;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("BCCE9584-9E2E-432d-8DA5-15D8AC7350DD")]
    public class Clip : SimpleActivity
    {
        private bool _mergeClipper = true;

        #region IActivity Member

        override public string DisplayName
        {
            get { return "Clip Features"; }
        }

        override public string CategoryName
        {
            get { return "Base"; }
        }

        override public List<IActivityData> Process()
        {
            IDatasetElement sElement = base.SourceDatasetElement;
            IFeatureClass sFc = base.SourceFeatureClass;

            TargetDatasetElement tElement = base.TargetDatasetElement;
            IDataset tDs = base.TargetDataset;
            //IFeatureDatabase tDatabase = base.TargetFeatureDatabase;

            IFeatureClass tFc = base.CreateTargetFeatureclass(sFc, sFc.Fields);
            IFeatureDatabase tDatabase = FeatureDatabase(tFc);

            return null;
        }

        #endregion
    }
}
