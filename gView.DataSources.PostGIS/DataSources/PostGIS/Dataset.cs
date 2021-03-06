using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.FDB;
using System.Data.Common;
using gView.Framework.system;
using gView.Framework.OGC.DB;

namespace gView.DataSources.PostGIS
{
    [UseDatasetNameCase(DatasetNameCase.classNameLower)]
    [RegisterPlugIn("206CF40B-D4D9-4e85-B872-D2E63C3556BA")]
    public class PostGISDataset : gView.Framework.OGC.DB.OgcSpatialDataset, IPlugInDependencies
    {
        DbProviderFactory _factory = null;
        int _majorVersion = 1;

        public PostGISDataset()
        {
            this.ConnectionStringChanged += new ConnectionStringChangedEventHandler(PostGISDataset_ConnectionStringChanged);
        }

        private PostGISDataset(DbProviderFactory factory)
        {
            this.ConnectionStringChanged += new ConnectionStringChangedEventHandler(PostGISDataset_ConnectionStringChanged);
            _factory = factory;
        }

        public override DbProviderFactory ProviderFactory
        {
            get { return _factory; }
        }

        public override string DbDictionary(IField field)
        {
            switch (field.type)
            {
                case FieldType.Shape:
                    return String.Empty;
                case FieldType.ID:
                    return "serial PRIMARY KEY";
                case FieldType.smallinteger:
                    return "int2";
                case FieldType.integer:
                    return "int4";
                case FieldType.biginteger:
                    return "int8";
                case FieldType.Float:
                    return "float4";
                case FieldType.Double:
                    return "float8";
                case FieldType.boolean:
                    return "bool";
                case FieldType.character:
                    return "varchar(1)";
                case FieldType.Date:
                    return "time";
                case FieldType.String:
                    return "varchar(" + ((field.size > 0) ? field.size : 256).ToString() + ")";
                default:
                    return "varchar(256)";
            }
        }

        public override string SelectReadSchema(string tableName)
        {
            return base.SelectReadSchema(tableName) + " limit 0";
        }

        protected override object ShapeParameterValue(OgcSpatialFeatureclass fClass, IGeometry shape, int srid, out bool AsSqlParameter)
        {
            AsSqlParameter = true;

            byte[] geometry = gView.Framework.OGC.OGC.GeometryToWKB(shape, srid, gView.Framework.OGC.OGC.WkbByteOrder.Ndr, fClass.GeometryTypeString);
            string geometryString = gView.Framework.OGC.OGC.BytesToHexString(geometry);

            return geometryString;
        }

        protected override string InsertShapeParameterExpression(OgcSpatialFeatureclass featureClass, IGeometry shape)
        {
            if (shape is IPolygon)
            {
                return "ST_MakeValid({0})";
            }
            else
            {
                return base.InsertShapeParameterExpression(featureClass, shape);
            }
        }

        void PostGISDataset_ConnectionStringChanged(gView.Framework.OGC.DB.OgcSpatialDataset sender, string provider)
        {
            try
            {
                switch (provider.ToLower())
                {
                    case "odbc":
                        _factory = DbProviderFactories.GetFactory("System.Data.Odbc");
                        break;
                    case "oledb":
                        _factory = DbProviderFactories.GetFactory("System.Data.OleDb");
                        break;
                    default:
                        //_factory = DbProviderFactories.GetFactory("Npgsql");
                        _factory = Npgsql.NpgsqlFactory.Instance;
                        break;
                }

                #region Version
                try
                {
                    object obj = base.ExecuteFunction("select postgis_version()");
                    if (obj is string)
                    {
                        if (obj.ToString().StartsWith("2."))
                            _majorVersion = 2;
                        else
                            _majorVersion = 1;
                    }
                }
                catch
                {
                    _majorVersion = 1;
                }
                #endregion
            }
            catch
            {
                _factory = null;
            }
        }

        protected override gView.Framework.OGC.DB.OgcSpatialDataset CreateInstance()
        {
            return new PostGISDataset(_factory);
        }

        public override string DbTableName(string tableName)
        {
            string schema = GetTableDbSchema(tableName);
            string tabName = "\"" + tableName.Replace(".", "\".\"") + "\"";  // falls tablename schon schema enth�lt -> . durch "." ersetzen -> "schema"."tablename"

            return String.IsNullOrEmpty(schema) ? tabName : "\"" + schema + "\"" + "." + tabName;
        }

        protected override string AddGeometryColumn(string schemaName, string tableName, string colunName, string srid, string geomTypeString)
        {
            if (_majorVersion == 2)
            {
                return "SELECT " + DbSchemaPrefix + "AddGeometryColumn ('" + schemaName + "','" + tableName + "','" + colunName + "','" + srid + "','" + geomTypeString + "','2',true)";
            }

            return base.AddGeometryColumn(schemaName, tableName, colunName, srid, geomTypeString);
        }

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return PostGISDataset.hasUnsolvedDependencies;
        }

        #endregion

        public static bool hasUnsolvedDependencies
        {
            get
            {
                try
                {
                    return Npgsql.NpgsqlFactory.Instance == null;
                    //if (DbProviderFactories.GetFactory("Npgsql") == null)
                    //    return true;

                    //return false;
                }
                catch (Exception ex)
                {
                    return true;
                }
            }
        }

        private Dictionary<string, string> _tableDbSchema = new Dictionary<string, string>();
        internal string GetTableDbSchema(string tableName)
        {
            try
            {
                if (tableName.Contains("."))  // Tablename includes schema
                    return String.Empty;

                if (_tableDbSchema.ContainsKey(tableName))
                    return _tableDbSchema[tableName];

                using (DbConnection conn = _factory.CreateConnection())
                {
                    conn.ConnectionString = this.ConnectionString;
                    DbCommand cmd = _factory.CreateCommand();
                    cmd.CommandText = "SELECT n.nspname as \"Schema\" FROM pg_catalog.pg_class c LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace WHERE c.relname='" + tableName + "'";
                    cmd.Connection = conn;

                    conn.Open();
                    object obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        string schema = obj.ToString();
                        schema = (schema != schema.ToLower()) ? "\"" + schema + "\"" : schema;
                        try
                        {
                            _tableDbSchema.Add(tableName, schema);
                        }
                        catch { }
                        return schema;
                    }

                    try
                    {
                        _tableDbSchema.Add(tableName, String.Empty);
                    }
                    catch { }
                    return String.Empty;
                }
            }
            catch { return String.Empty; }
        }

        public override bool CanEditFeatureClass(IFeatureClass fc, EditCommands command)
        {
            if (fc is OgcSpatialFeatureclass)
            {
                OgcSpatialFeatureclass ogcFc = (OgcSpatialFeatureclass)fc;
                if (ogcFc.GeometryTypeString.ToUpper().EndsWith("M") ||
                    ogcFc.GeometryTypeString.ToUpper().EndsWith("Z"))
                {
                    switch (command)
                    {
                        case EditCommands.Insert:
                            _errMsg = "Can't insert features for geometrytype " + ogcFc.GeometryTypeString + ".";
                            return false;
                        case EditCommands.Update:
                            _errMsg = "Can't update features for geometrytype " + ogcFc.GeometryTypeString + ".";
                            return false;
                    }
                }
                return true;

            }
            return false;
        }

        //        public override string PrimaryKeyField(string tableName)
        //        {
        //            string schema = "";
        //            if (tableName.Contains("."))
        //            {
        //                schema = tableName.Split('.')[0];
        //                tableName = tableName.Substring(schema.Length + 1);
        //            }

        //            if (!String.IsNullOrWhiteSpace(schema))
        //            {
        //                return @"SELECT
        //c.column_name
        //FROM
        //information_schema.table_constraints tc 
        //JOIN information_schema.constraint_column_usage AS ccu USING (constraint_schema, constraint_name) 
        //JOIN information_schema.columns AS c ON c.table_schema = tc.constraint_schema AND tc.table_name = c.table_name AND ccu.column_name = c.column_name
        //where constraint_type = 'PRIMARY KEY' and tc.table_schema='" + schema + "' and tc.table_name = '" + tableName + "'";
        //            }
        //            else
        //            {
        //                return @"SELECT
        //c.column_name
        //FROM
        //information_schema.table_constraints tc 
        //JOIN information_schema.constraint_column_usage AS ccu USING (constraint_schema, constraint_name) 
        //JOIN information_schema.columns AS c ON c.table_schema = tc.constraint_schema AND tc.table_name = c.table_name AND ccu.column_name = c.column_name
        //where constraint_type = 'PRIMARY KEY' and tc.table_name = '" + tableName + "'";
        //            }
        //        }

        //        public override string PrimaryKeyField(string tableName)
        //        {
        //            string schema = "";
        //            if (tableName.Contains("."))
        //            {
        //                schema = tableName.Split('.')[0];
        //                tableName = tableName.Substring(schema.Length + 1);
        //            }

        //            return @"SELECT               
        //  pg_attribute.attname, 
        //  format_type(pg_attribute.atttypid, pg_attribute.atttypmod) 
        //FROM pg_index, pg_class, pg_attribute, pg_namespace 
        //WHERE 
        //  pg_class.oid = '" + tableName + @"'::regclass AND 
        //  indrelid = pg_class.oid AND 
        //  nspname = '" + (String.IsNullOrWhiteSpace(schema) ? "public" : schema) + @"' AND 
        //  pg_class.relnamespace = pg_namespace.oid AND 
        //  pg_attribute.attrelid = pg_class.oid AND 
        //  pg_attribute.attnum = any(pg_index.indkey)
        // AND indisprimary";
        //        }

        public override string IntegerPrimaryKeyField(string tableName)
        {
            string schema = "public";
            if (tableName.Contains("."))
            {
                schema = tableName.Split('.')[0];
                tableName = tableName.Substring(schema.Length + 1);
            }

            return @"SELECT a.attname
FROM pg_index i
JOIN pg_attribute a ON a.attrelid=i.indrelid AND a.attnum=ANY(i.indkey)
WHERE i.indrelid='" + schema + "." + tableName + @"'::regclass AND i.indisprimary AND format_type(a.atttypid, a.atttypmod)='integer'";
        }
    }
}
