using gView.Framework.Data;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.Geometry;
using gView.Framework.OGC.DB;
using gView.DataSources.OracleGeometry.Types;

namespace gView.DataSources.OracleGeometry
{
    [UseDatasetNameCase(DatasetNameCase.classNameUpper)]
    //[RegisterPlugIn("1C567195-B2C7-4202-8DC0-5FF27C876AF3")]
    public class Dataset : gView.Framework.OGC.DB.OgcSpatialDataset
    {
        protected DbProviderFactory _factory = null;

        public Dataset()
        {
            try
            {
                _factory = new Oracle.ManagedDataAccess.Client.OracleClientFactory();
            }
            catch
            {
                _factory = null;
            }
        }

        protected Dataset(DbProviderFactory factory)
        {
            _factory = factory;
        }

        public override DbProviderFactory ProviderFactory
        {
            get { return _factory; }
        }

        protected override gView.Framework.OGC.DB.OgcSpatialDataset CreateInstance()
        {
            return new Dataset(_factory);
        }

        public override string OgcDictionary(string ogcExpression)
        {
            switch(ogcExpression.ToLower())
            {
                case "gview_id":
                case "gid":
                    return "ID";
                case "the_geom":
                    return "SHAPE";
            }
            return base.OgcDictionary(ogcExpression);
        }

        public override string DbDictionary(IField field)
        {
            switch (field.type)
            {
                case FieldType.Shape:
                    return "SDO_GEOMETRY";
                case FieldType.ID:
                    return "NUMBER NOT NULL"; // GENERATED ALWAYS as IDENTITY(START with 1 INCREMENT by 1)";
                case FieldType.smallinteger:
                    return "SHORTINTEGER NULL";
                case FieldType.integer:
                    return "INTEGER NULL";
                case FieldType.biginteger:
                    return "LONGINTEGER NULL";
                case FieldType.Float:
                    return "NUMBER NULL";
                case FieldType.Double:
                    return "NUMBER NULL";
                case FieldType.boolean:
                    return "NUNBER(1,0) NULL";
                case FieldType.character:
                    return "CHAR(1) NULL";
                case FieldType.Date:
                    return "DATE NULL";
                case FieldType.String:
                    return "NVARCHAR2(" + field.size + ") NULL";
                default:
                    return "NVARCHAR2(255) NULL";
            }
        }

        protected string IDFieldName(string tabName)
        {
            try
            {
                using (DbConnection conn = this.ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    DbCommand command = this.ProviderFactory.CreateCommand();
                    // https://stackoverflow.com/questions/9016578/how-to-get-primary-key-column-in-oracle
                    command.CommandText = "SELECT column_name FROM all_cons_columns WHERE constraint_name = (SELECT constraint_name FROM all_constraints WHERE UPPER(table_name) = UPPER('" + tabName + "') AND CONSTRAINT_TYPE = 'P');";
                    command.Connection = conn;

                    string idFieldName = command.ExecuteScalar() as string;
                    return idFieldName;
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return String.Empty;
            }
        }

        protected override string CreateGidSequence(string tableName)
        {
            return @"CREATE SEQUENCE seq_id_" + tableName + @"
 START WITH     1
 INCREMENT BY   1
 NOCACHE
 NOCYCLE";
        }

        protected override string CreateGidTrigger(string tableName, string gid)
        {
            return @"CREATE OR REPLACE TRIGGER trg_id_" + tableName + @"
BEFORE INSERT OR UPDATE ON " + tableName + @"
FOR EACH ROW
BEGIN
   SELECT seq_id_" + tableName + @".NextVal
   INTO :new." + gid + @"
   FROM dual;
END;";
        }

        protected override string AddGeometryColumn(string schemaName, string tableName, string colunName, string srid, string geomTypeString)
        {
            // DoTo: Create Table
            return String.Empty;
        }

        protected override string DropGeometryTable(string schemaName, string tableName)
        {
            return "drop table " + tableName + ";drop sequence seq_id_" + tableName;
        }

        public override DbCommand SelectSpatialReferenceIds(gView.Framework.OGC.DB.OgcSpatialFeatureclass fc)
        {
            //string cmdText = "select distinct " + fc.ShapeFieldName + ".STSrid as srid from " + fc.Name + " where " + fc.ShapeFieldName + " is not null";
            //DbCommand command = this.ProviderFactory.CreateCommand();
            //command.CommandText = cmdText;

            //return command;
            return null;
        }

        protected override string DbColumnName(string colName)
        {
            return colName;
        }

        protected override string DbParameterName(string name)
        {
            return ":" + name;
        }

        override protected object ShapeParameterValue(OgcSpatialFeatureclass featureClass, IGeometry shape, int srid, out bool AsSqlParameter)
        {
            AsSqlParameter = false;

            return SdoGeometry.FromGeometry(shape, srid);
        }

        #region IDataset 

        public override List<IDatasetElement> Elements
        {
            get
            {
                if (_layers == null || _layers.Count == 0)
                {
                    List<IDatasetElement> layers = new List<IDatasetElement>();
                    DataTable tables = new DataTable(), views = new DataTable();
                    try
                    {
                        using (DbConnection conn = this.ProviderFactory.CreateConnection())
                        {
                            conn.ConnectionString = _connectionString;
                            conn.Open();

                            DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                            adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                            adapter.SelectCommand.CommandText = @"SELECT TABLE_NAME, column_name from user_tab_columns WHERE data_type='SDO_GEOMETRY' ORDER BY TABLE_NAME";
                            adapter.SelectCommand.Connection = conn;
                            adapter.Fill(tables);

                            // DoTo: Views

                            conn.Close();
                        }

                    }
                    catch (Exception ex)
                    {
                        _errMsg = ex.Message;
                        return layers;
                    }

                    foreach (DataRow row in tables.Rows)
                    {
                        try
                        {
                            Featureclass fc = new Featureclass(this,
                                row["TABLE_NAME"].ToString(),
                                String.Empty, //IDFieldName(row["TABLE_NAME"].ToString()),
                                row["column_name"].ToString(), false);

                            if (fc.Fields.Count > 0)
                                layers.Add(new DatasetElement(fc));
                        }
                        catch { }
                    }
                    foreach (DataRow row in views.Rows)
                    {
                        try
                        {
                            Featureclass fc = new Featureclass(this,
                                row["TABLE_NAME"].ToString(),
                                String.Empty, // IDFieldName(row["TABLE_NAME"].ToString()),
                                row["column_name"].ToString(), true);

                            if (fc.Fields.Count > 0)
                                layers.Add(new DatasetElement(fc));
                        }
                        catch { }
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
                try
                {
                    DataTable tab = new DataTable();
                    using (DbConnection conn = this.ProviderFactory.CreateConnection())
                        {
                            conn.ConnectionString = _connectionString;
                            conn.Open();

                            DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                            adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                            adapter.SelectCommand.CommandText = @"SELECT TABLE_NAME, column_name from user_tab_columns WHERE data_type='SDO_GEOMETRY' and TABLE_NAME='" + title + "'";
                            adapter.SelectCommand.Connection = conn;
                            adapter.Fill(tab);

                            // DoTo: Views

                            conn.Close();
                    }
                    if (tab.Rows.Count != 1)
                    {
                        _errMsg = "Layer not found: " + title;
                        return null;
                    }

                    Featureclass fc = new Featureclass(this,
                                tab.Rows[0]["TABLE_NAME"].ToString(),
                                String.Empty, //IDFieldName(row["TABLE_NAME"].ToString()),
                                tab.Rows[0]["column_name"].ToString(), false);

                    if (fc.Fields.Count > 0)
                        return new DatasetElement(fc);

                    return null;
                }
                catch (Exception ex)
                {
                    _errMsg = ex.Message;
                    return null;
                }
            }
        }

        #endregion
    }
}
