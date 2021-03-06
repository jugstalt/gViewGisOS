﻿using gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.OGC.DB;
using gView.OGC.Framework.OGC.DB;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde
{
    [gView.Framework.system.RegisterPlugIn("F7394B37-1397-4914-B1D0-5A03B11D2949")]
    public class SdeDataset : gView.Framework.OGC.DB.OgcSpatialDataset
    {
        protected DbProviderFactory _factory = null;
        protected IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        internal RepoProvider RepoProvider = null;
        
        public SdeDataset()
            : base()
        {
            try
            {
                _factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            }
            catch
            {
                _factory = null;
            }
        }

        protected SdeDataset(DbProviderFactory factory)
            : base()
        {
            _factory = factory;
        }

        protected override gView.Framework.OGC.DB.OgcSpatialDataset CreateInstance()
        {
            return new SdeDataset(_factory);
        }

        public override DbProviderFactory ProviderFactory
        {
            get { return _factory; }
        }

        public override bool Open()
        {
            var repo = new RepoProvider();
            repo.Init(_connectionString);
            RepoProvider = repo;

            return base.Open();
        }

        public override string OgcDictionary(string ogcExpression)
        {
            switch (ogcExpression.ToLower())
            {
                case "gid":
                    return "OBJECTID";
                case "the_geom":
                    return "SHAPE";
                case "geometry_columns":
                case "geometry_columns.f_table_name":
                case "geometry_columns.f_geometry_column":
                case "geometry_columns.f_table_catalog":
                case "geometry_columns.f_table_schema":
                case "geometry_columns.coord_dimension":
                case "geometry_columns.srid":
                    return Field.shortName(ogcExpression).ToUpper();
                case "geometry_columns.type":
                    return "GEOMETRY_TYPE";
                case "gview_id":
                    return "gview_id";
            }
            return Field.shortName(ogcExpression);
        }

        public override string DbDictionary(IField field)
        {
            switch (field.type)
            {
                case FieldType.Shape:
                    return "[GEOMETRY]";
                case FieldType.ID:
                    return "[int] IDENTITY(1,1) NOT NULL CONSTRAINT KEY_" + System.Guid.NewGuid().ToString("N") + "_" + field.name + " PRIMARY KEY CLUSTERED";
                case FieldType.smallinteger:
                    return "[int] NULL";
                case FieldType.integer:
                    return "[int] NULL";
                case FieldType.biginteger:
                    return "[bigint] NULL";
                case FieldType.Float:
                    return "[float] NULL";
                case FieldType.Double:
                    return "[float] NULL";
                case FieldType.boolean:
                    return "[bit] NULL";
                case FieldType.character:
                    return "[nvarchar] (1) NULL";
                case FieldType.Date:
                    return "[datetime] NULL";
                case FieldType.String:
                    return "[nvarchar](" + field.size + ")";
                default:
                    return "[nvarchar] (255) NULL";
            }
        }

        protected override object ShapeParameterValue(OgcSpatialFeatureclass fClass, gView.Framework.Geometry.IGeometry shape, int srid, out bool AsSqlParameter)
        {
            if (shape is IPolygon)
            {
                #region Check Polygon Rings
                IPolygon p = new Polygon();
                for (int i = 0; i < ((IPolygon)shape).RingCount; i++)
                {
                    IRing ring = ((IPolygon)shape)[i];
                    if (ring != null && ring.Area > 0D)
                    {
                        p.AddRing(ring);
                    }
                }

                if (p.RingCount == 0)
                {
                    AsSqlParameter = true;
                    return null;
                }
                shape = p;
                #endregion
            }
            else if (shape is IPolyline)
            {
                #region Check Polyline Paths
                IPolyline l = new Polyline();
                for (int i = 0; i < ((IPolyline)shape).PathCount; i++)
                {
                    IPath path = ((IPolyline)shape)[i];
                    if (path != null && path.Length > 0D)
                    {
                        l.AddPath(path);
                    }
                }

                if (l.PathCount == 0)
                {
                    AsSqlParameter = true;
                    return null;
                }
                shape = l;
                #endregion
            }

            AsSqlParameter = false;

            //return gView.Framework.OGC.OGC.GeometryToWKB(shape, gView.Framework.OGC.OGC.WkbByteOrder.Ndr);
            string geometryString =
                (shape is IPolygon) ?
                "geometry::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ").MakeValid()" :
                "geometry::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ")";
            return geometryString;
            //return "geometry::STGeomFromText('" + geometryString + "',0)";
        }

        public override DbCommand SelectCommand(gView.Framework.OGC.DB.OgcSpatialFeatureclass fc, IQueryFilter filter, out string shapeFieldName)
        {
            shapeFieldName = String.Empty;

            DbCommand command = this.ProviderFactory.CreateCommand();

            filter.fieldPrefix = "[";
            filter.fieldPostfix = "]";

            if (filter.SubFields == "*")
            {
                filter.SubFields = "";

                foreach (IField field in fc.Fields.ToEnumerable())
                {
                    filter.AddField(field.name);
                }
                filter.AddField(fc.IDFieldName);
                filter.AddField(fc.ShapeFieldName);
            }
            else
            {
                filter.AddField(fc.IDFieldName);
            }

            string where = String.Empty;
            if (filter is ISpatialFilter && ((ISpatialFilter)filter).Geometry != null)
            {
                ISpatialFilter sFilter = filter as ISpatialFilter;

                int srid = 0;
                try
                {
                    if (fc.SpatialReference != null && fc.SpatialReference.Name.ToLower().StartsWith("epsg:"))
                    {
                        srid = Convert.ToInt32(fc.SpatialReference.Name.Split(':')[1]);
                    }
                }
                catch { }

                if (sFilter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects ||
                    sFilter.Geometry is IEnvelope)
                {
                    IEnvelope env = sFilter.Geometry.Envelope;

                    where = fc.ShapeFieldName + ".Filter(";
                    where += "geometry::STGeomFromText('POLYGON((";
                    where += env.minx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + ",";

                    where += env.minx.ToString(_nhi) + " ";
                    where += env.maxy.ToString(_nhi) + ",";

                    where += env.maxx.ToString(_nhi) + " ";
                    where += env.maxy.ToString(_nhi) + ",";

                    where += env.maxx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + ",";

                    where += env.minx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + "))'," + srid + "))=1";
                }
                else if (sFilter.Geometry != null)
                {
                    IEnvelope env = sFilter.Geometry.Envelope;

                    where = fc.ShapeFieldName + ".STIntersects(";
                    where += "geometry::STGeomFromText('POLYGON((";
                    where += env.minx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + ",";

                    where += env.minx.ToString(_nhi) + " ";
                    where += env.maxy.ToString(_nhi) + ",";

                    where += env.maxx.ToString(_nhi) + " ";
                    where += env.maxy.ToString(_nhi) + ",";

                    where += env.maxx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + ",";

                    where += env.minx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + "))'," + srid + "))=1";
                }
                filter.AddField(fc.ShapeFieldName);
            }
            string filterWhereClause = (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause;

            StringBuilder fieldNames = new StringBuilder();
            foreach (string fieldName in filter.SubFields.Split(' '))
            {
                if (fieldNames.Length > 0) fieldNames.Append(",");
                if (fieldName == "[" + fc.ShapeFieldName + "]")
                {
                    fieldNames.Append(fc.ShapeFieldName + ".STAsBinary() as temp_geometry");
                    shapeFieldName = "temp_geometry";
                }
                else
                {
                    fieldNames.Append(fieldName);
                }
            }

            command.CommandText = "SELECT " + fieldNames + " FROM " + fc.Name;
            if (!String.IsNullOrEmpty(where))
            {
                command.CommandText += " WHERE " + where + ((filterWhereClause != "") ? " AND " + filterWhereClause : "");
            }
            else if (!String.IsNullOrEmpty(filterWhereClause))
            {
                command.CommandText += " WHERE " + filterWhereClause;
            }

            //command.CommandText = "SELECT " + fieldNames + " FROM " + table + ((filterWhereClause != "") ? " WHERE " + filterWhereClause : "");

            return command;
        }

        public override IEnvelope FeatureClassEnvelope(IFeatureClass fc)
        {
            if (RepoProvider == null)
                throw new Exception("Repository not initialized");

            return RepoProvider.FeatureClassEnveolpe(fc);
        }

        public override List<IDatasetElement> Elements
        {
            get
            {
                if (RepoProvider == null)
                    throw new Exception("Repository not initialized");

                if (_layers == null || _layers.Count == 0)
                {
                    List<IDatasetElement> layers = new List<IDatasetElement>();

                    foreach (var sdeLayer in RepoProvider.Layers)
                    {
                        layers.Add(new DatasetElement(
                            new SdeFeatureClass(this, sdeLayer.Owner + "." + sdeLayer.TableName)));
                    }

                    _layers = layers;
                }

                return _layers;
            }
        }

        public override IDatasetElement this[string title]
        {
            get
            {
                if (RepoProvider == null)
                    throw new Exception("Repository not initialized");

                title = title.ToLower();
                var sdeLayer = RepoProvider.Layers.Where(l => (l.Owner + "." + l.TableName).ToLower() == title).FirstOrDefault();

                if (sdeLayer != null)
                {
                    return new DatasetElement(new SdeFeatureClass(this, sdeLayer.Owner + "." + sdeLayer.TableName));
                }

                return null;
            }
        }
    }
}
