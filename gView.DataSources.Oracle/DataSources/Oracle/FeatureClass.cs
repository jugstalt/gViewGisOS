using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.OracleGeometry
{
    public class Featureclass : gView.Framework.OGC.DB.OgcSpatialFeatureclass
    {
        public Featureclass(Dataset dataset, string name, string idFieldName, string shapeFieldName, bool isView)
        {
            _name = name;
            _idfield = idFieldName;
            _shapefield = shapeFieldName;
            _geomType = geometryType.Unknown;

            _dataset = dataset;

            ReadSchema();

            if (String.IsNullOrEmpty(_idfield) && _fields.Count > 0 && _dataset != null)
            {
                Field field = _fields[0] as Field;
                if (field != null)
                {
                    if ((field.type == FieldType.integer || field.type == FieldType.biginteger || field.type == FieldType.ID)
                        && field.name.ToLower() == _dataset.OgcDictionary("gview_id").ToLower())
                        _idfield = field.name;
                    ((Field)field).type = FieldType.ID;
                }
            }

            //base._geomType = geometryType.Polygon;

            if (_sRef == null)
                _sRef = gView.Framework.OGC.DB.OgcSpatialFeatureclass.TrySelectSpatialReference(dataset, this);
        }

        public override ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
        }
    }
}
