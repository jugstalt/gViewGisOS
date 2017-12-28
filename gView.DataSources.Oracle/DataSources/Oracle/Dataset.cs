using gView.Framework.Data;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.OracleGeometry
{
    [RegisterPlugIn("1C567195-B2C7-4202-8DC0-5FF27C876AF3")]
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

        public override string DbDictionary(IField field)
        {
            switch (field.type)
            {
                case FieldType.Shape:
                    return "SDO_GEOMETRY";
                case FieldType.ID:
                    return "[int] IDENTITY(1,1) NOT NULL CONSTRAINT KEY_" + System.Guid.NewGuid().ToString("N") + "_" + field.name + " PRIMARY KEY CLUSTERED";
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
    }
}
