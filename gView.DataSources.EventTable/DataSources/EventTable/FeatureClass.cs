using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Db;
using System.Data;

namespace gView.DataSources.EventTable
{
    class FeatureClass : IFeatureClass
    {
        private Fields _fields = null;
        private EventTableConnection _etcon;
        private Dataset _dataset;
        private IEnvelope _env = null;

        public FeatureClass(Dataset dataset, EventTableConnection etconn)
        {
            _etcon = etconn;
            _dataset = dataset;

            if (_etcon != null)
            {
                CommonDbConnection conn = new CommonDbConnection();
                conn.ConnectionString2 = _etcon.DbConnectionString.ConnectionString;
                if (conn.GetSchema(_etcon.TableName))
                {
                    _fields = new Fields(conn.schemaTable);
                    IField idfield = _fields.FindField(_etcon.IdFieldName);
                    if (idfield is Field) ((Field)idfield).type = FieldType.ID;
                }
                DataTable tab = conn.Select("MIN(" + _etcon.XFieldName + ") as minx,MAX(" + _etcon.XFieldName + ") as maxx,MIN(" + _etcon.YFieldName + ") as miny,MAX(" + _etcon.YFieldName + ") as maxy", _etcon.TableName);
                if (tab != null && tab.Rows.Count == 1)
                {
                    try
                    {
                        _env = new Envelope(
                            Convert.ToDouble(tab.Rows[0]["minx"]),
                            Convert.ToDouble(tab.Rows[0]["miny"]),
                            Convert.ToDouble(tab.Rows[0]["maxx"]),
                            Convert.ToDouble(tab.Rows[0]["maxy"]));
                    }
                    catch
                    {
                        _env = new Envelope();
                    }
                }
            }
        }

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return "#SHAPE#"; }
        }

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get { return _env; }
        }

        public int CountFeatures
        {
            get { return 0; }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            if (filter is ISpatialFilter)
            {
                filter = SpatialFilter.Project(filter as ISpatialFilter, this.SpatialReference);
            }

            return new FeatureCursor(_etcon, filter);
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            return GetFeatures(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            if (String.IsNullOrEmpty(this.IDFieldName) || filter == null)
                return null;

            filter.SubFields = this.IDFieldName;

            using (IFeatureCursor cursor = this.GetFeatures(filter))
            {
                IFeature feat;

                SpatialIndexedIDSelectionSet selSet = new SpatialIndexedIDSelectionSet(this.Envelope);
                while ((feat = cursor.NextFeature) != null)
                {
                    selSet.AddID(feat.OID, feat.Shape);
                }

                return selSet;
            }         
        }

        public IFields Fields
        {
            get { return _fields; }
        }

        public IField FindField(string name)
        {
            if (_fields != null)
                return _fields.FindField(name);
            return null;
        }

        public string IDFieldName
        {
            get
            {
                if (_etcon != null)
                    return _etcon.IdFieldName;

                return String.Empty;
            }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get
            {
                if (_etcon!=null)
                    return _etcon.TableName;

                return String.Empty;
            }
        }

        public string Aliasname
        {
            get
            {
                if (_etcon!=null)
                    return _etcon.TableName;

                return String.Empty;
            }
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

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get
            {
                if (_etcon != null)
                    return _etcon.SpatialReference;
                return null;
            }
        }

        public gView.Framework.Geometry.geometryType GeometryType
        {
            get { return geometryType.Point; }
        }

        #endregion
    }
}
