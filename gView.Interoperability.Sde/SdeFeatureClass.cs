using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using gView.SDEWrapper;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Interoperability.Sde
{
    internal class SdeFeatureClass : IFeatureClass
    {
        private SdeDataset _dataset = null;
        private string _name = String.Empty, _shapeFieldName = String.Empty, _idFieldName = String.Empty, _errMsg = String.Empty;
        private IEnvelope _envelope = null;
        private geometryType _geomType = geometryType.Unknown;
        private Fields _fields = null;

        public SdeFeatureClass(SdeDataset dataset, SE_CONNECTION connection, SE_LAYERINFO layerInfo)
        {
            _dataset = dataset;

            byte[] tableName = new byte[CONST.SE_QUALIFIED_TABLE_NAME];
            byte[] columnName = new byte[CONST.SE_MAX_COLUMN_LEN];
            byte[] shapeColumnName = new byte[CONST.SE_MAX_COLUMN_LEN];

            System.Int32 status = Wrapper92.SE_layerinfo_get_spatial_column(layerInfo, tableName, shapeColumnName);
            if (status != 0) return;

            _name = Functions.GetASCIIString(tableName);

            //_shapeFildName mit den Felder abfragen, weils sonst oft Probleme mit Groﬂ/Kleinschreibung der Felder gibt...
            //_shapeFieldName = Functions.GetASCIIString(shapeColumnName); 

            SE_ENVELOPE sdeEnvelope = new SE_ENVELOPE();
            status = Wrapper92.SE_layerinfo_get_envelope(layerInfo, ref sdeEnvelope);
            if (status == 0)
            {
                _envelope = new Envelope(sdeEnvelope.minx, sdeEnvelope.miny, sdeEnvelope.maxx, sdeEnvelope.maxy);
            }

            System.Int32 shape_types = 0;
            status = Wrapper92.SE_layerinfo_get_shape_types(layerInfo, ref shape_types);
            if (status == 0)
            {
                if ((shape_types & CONST.SE_NIL_TYPE_MASK) != 0)
                {
                    _geomType = geometryType.Unknown;
                }
                if ((shape_types & CONST.SE_POINT_TYPE_MASK) != 0)
                {
                    _geomType = geometryType.Point;
                }
                if ((shape_types & CONST.SE_LINE_TYPE_MASK) != 0)
                {
                    _geomType = geometryType.Polyline;
                }
                if ((shape_types & CONST.SE_SIMPLE_LINE_TYPE_MASK) != 0)
                {
                    _geomType = geometryType.Polyline;
                }
                if ((shape_types & CONST.SE_AREA_TYPE_MASK) != 0)
                {
                    _geomType = geometryType.Polygon;
                }
                //if ((shape_types & CONST.SE_UNVERIFIED_SHAPE_MASK) != 0)
                //{
                //    _geomType = geometryType.Unknown;
                //}
                //if ((shape_types & CONST.SE_MULTIPART_TYPE_MASK) != 0)
                //{
                //    _geomType = geometryType.Aggregate;
                //}
            }


            // IDField

            IntPtr regInfo = new IntPtr(0);
            status = Wrapper92.SE_reginfo_create(ref regInfo);
            if (status == 0)
            {
                try
                {
                    if (Wrapper92.SE_registration_get_info(connection, _name, regInfo) == 0)
                    {
                        byte[] buffer = new byte[CONST.SE_MAX_COLUMN_LEN];
                        System.Int32 idColType = 0;
                        if (Wrapper92.SE_reginfo_get_rowid_column(regInfo, buffer, ref idColType) == 0)
                        {
                            _idFieldName = Functions.GetASCIIString(buffer);
                        }
                    }
                }
                catch { }
                Wrapper92.SE_reginfo_free(regInfo);
            }

            // Felder auslesen
            _fields = new Fields();

            IntPtr ptr = new IntPtr(0);
            System.Int16 numFields = 0;

            status = Wrapper92.SE_table_describe(connection, _name, ref numFields, ref ptr);
            if (status == 0)
            {
                try
                {
                    unsafe
                    {
                        byte* columnDefs = (byte*)ptr;

                        for (int i = 0; i < numFields; i++)
                        {
                            SE_COLUMN_DEF colDef = (SE_COLUMN_DEF)Marshal.PtrToStructure((IntPtr)columnDefs, typeof(SE_COLUMN_DEF));

                            string colName = Functions.GetASCIIString(colDef.column_name);
                            FieldType colType = FieldType.unknown;

                            switch (colDef.sde_type)
                            {
                                case CONST.SE_SMALLINT_TYPE:
                                    colType = FieldType.smallinteger;
                                    break;
                                case CONST.SE_INTEGER_TYPE:
                                    colType = FieldType.integer;
                                    break;
                                case CONST.SE_FLOAT_TYPE:
                                    colType = FieldType.Float;
                                    break;
                                case CONST.SE_DOUBLE_TYPE:
                                    colType = FieldType.Double;
                                    break;
                                case CONST.SE_STRING_TYPE:
                                    colType = FieldType.String;
                                    break;
                                case CONST.SE_NSTRING_TYPE:
                                    colType = FieldType.NString;
                                    break;
                                case CONST.SE_BLOB_TYPE:
                                    colType = FieldType.binary;
                                    break;
                                case CONST.SE_DATE_TYPE:
                                    colType = FieldType.Date;
                                    break;
                                case CONST.SE_SHAPE_TYPE:
                                    colType = FieldType.Shape;
                                    if (String.IsNullOrEmpty(_shapeFieldName)) _shapeFieldName = colName;
                                    break;
                                case CONST.SE_RASTER_TYPE:
                                    break;
                                default:
                                    colType = FieldType.unknown;
                                    break;
                            }
                            if (colName == _idFieldName) colType = FieldType.ID;
                            _fields.Add(new Field(colName, colType, (int)colDef.size, (int)colDef.decimal_digits));

                            columnDefs += Marshal.SizeOf(colDef);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errMsg = ex.Message;
                }

                if(String.IsNullOrEmpty(_shapeFieldName)) // nur wenn bei den Felder nix gefunden wurde...
                    _shapeFieldName = Functions.GetASCIIString(shapeColumnName); 

                Wrapper92.SE_table_free_descriptions(ptr);
            }
        }

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return _shapeFieldName; }
        }

        public IEnvelope Envelope
        {
            get { return _envelope; }
        }

        public int CountFeatures
        {
            get { return -1; }
        }

        /*
        public IFeature GetFeature(int id, getFeatureQueryType type)
        {
            if (_dataset == null) return null;

            string sql = this.IDFieldName + "=" + id.ToString();
            QueryFilter filter = new QueryFilter();
            filter.WhereClause = sql;

            switch (type)
            {
                case getFeatureQueryType.All:
                    filter.SubFields = "*";
                    break;
                case getFeatureQueryType.Attributes:
                    if (this.Fields == null) return null;
                    foreach (IField field in this.Fields)
                    {
                        if (field.type == FieldType.Shape) continue;
                        filter.AddField(field.name);
                    }
                    break;
                case getFeatureQueryType.Geometry:
                    if (filter.SubFields != "*") filter.AddField(ShapeFieldName);
                    break;
            }

            SdeFeatureCursor cursor = new SdeFeatureCursor(_dataset, this, filter);
            return cursor.NextFeature;
        }

        public IFeatureCursor GetFeatures(List<int> ids, getFeatureQueryType type)
        {
            if (_dataset == null) return null;

            StringBuilder sql = new StringBuilder();
            sql.Append(this.IDFieldName + " in (");
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0) sql.Append(",");
                sql.Append(ids[i].ToString());
            }
            sql.Append(")");
            QueryFilter filter = new QueryFilter();
            filter.WhereClause = sql.ToString();

            switch (type)
            {
                case getFeatureQueryType.All:
                    filter.SubFields = "*";
                    break;
                case getFeatureQueryType.Attributes:
                    if (this.Fields == null) return null;
                    foreach (IField field in this.Fields)
                    {
                        if (field.type == FieldType.Shape) continue;
                        filter.AddField(field.name);
                    }
                    break;
                case getFeatureQueryType.Geometry:
                    filter.SubFields = this.ShapeFieldName;
                    break;
            }

            SdeFeatureCursor cursor = new SdeFeatureCursor(_dataset, this, filter);
            return cursor;
        }
        */

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            try
            {
                //if (filter is ISpatialFilter)
                //{
                //    ISpatialFilter filterIDs = filter.Clone() as ISpatialFilter;
                //    filterIDs.SubFields = this.IDFieldName;

                //    SdeFeatureCursor cursor = new SdeFeatureCursor(_dataset, this, filterIDs);
                //    if (cursor == null) return null;

                //    List<int> ids = new List<int>();
                //    IFeature feature;
                //    while ((feature = cursor.NextFeature) != null)
                //    {
                //        ids.Add(feature.OID);
                //    }
                //}
                SdeFeatureCursor cursor = new SdeFeatureCursor(_dataset, this, filter);
                return cursor;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return null;
            }
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            SdeFeatureCursor cursor = new SdeFeatureCursor(_dataset, this, filter);
            return cursor;
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            filter.SubFields = this.IDFieldName;

            using (SdeFeatureCursor cursor = new SdeFeatureCursor(_dataset, this, filter))
            {
                IFeature feat;

                IDSelectionSet selSet = new IDSelectionSet();
                while ((feat = cursor.NextFeature) != null)
                {
                    selSet.AddID(feat.OID);
                }

                return selSet;
            }

        }

        public IFields Fields
        {
            get { return _fields == null ? new Fields() : _fields; }
        }

        public IField FindField(string name)
        {
            if (_fields == null) return null;

            foreach (IField field in _fields.ToEnumerable())
            {
                if (field.name == name ||
                    this.Name + "." + field.name == name) return field;
            }

            return null;
        }

        public string IDFieldName
        {
            get { return _idFieldName; }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IGeometryDef Member

        public bool HasZ
        {
            get { return false; }
        }

        public bool HasM
        {
            get { return false; }
        }

        public ISpatialReference SpatialReference
        {
            get { return null; }
        }

        public geometryType GeometryType
        {
            get { return _geomType; }
        }

        public GeometryFieldType GeometryFieldType
        {
            get
            {
                return GeometryFieldType.Default;
            }
        }
        #endregion
    }
}
