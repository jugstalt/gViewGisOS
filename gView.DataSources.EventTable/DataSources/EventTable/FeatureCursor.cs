using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Db;
using gView.Framework.Geometry;
using System.Data;
using System.Data.Common;

namespace gView.DataSources.EventTable
{
    class FeatureCursor : gView.Framework.Data.FeatureCursor
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        private EventTableConnection _etcon;
        //private DataTable _tab = null;
        private int _pos = 0;
        private bool _addShape = false;
        private DbDataReader _dbReader = null;
        private DbConnection _dbConnection = null;
        private ISpatialFilter _spatialFilter = null;

        public FeatureCursor(EventTableConnection etconn, IQueryFilter filter)
            : base(
                    (etconn != null ? etconn.SpatialReference : null),
                    (filter != null ? filter.FeatureSpatialReference : null))
        {
            _etcon = etconn;
            if (etconn != null)
            {
                CommonDbConnection conn = new CommonDbConnection();
                conn.ConnectionString2 = etconn.DbConnectionString.ConnectionString;

                string appendWhere = String.Empty;
                if (filter is ISpatialFilter &&
                    ((ISpatialFilter)filter).Geometry != null)
                {
                    if (!(((ISpatialFilter)filter).Geometry is Envelope))
                    {
                        _addShape = true;
                        filter.AddField("#SHAPE#");
                        _spatialFilter = (ISpatialFilter)filter;
                    }
                    IEnvelope env = ((ISpatialFilter)filter).Geometry.Envelope;
                    appendWhere =
                        etconn.XFieldName + ">=" + env.minx.ToString(_nhi) + " AND " +
                        etconn.XFieldName + "<=" + env.maxx.ToString(_nhi) + " AND " +
                        etconn.YFieldName + ">=" + env.miny.ToString(_nhi) + " AND " +
                        etconn.YFieldName + "<=" + env.maxy.ToString(_nhi);
                }
                if (filter is IRowIDFilter)
                {
                    IRowIDFilter idFilter = (IRowIDFilter)filter;
                    appendWhere = idFilter.RowIDWhereClause;
                }

                string where = (filter != null) ? filter.WhereClause : String.Empty;
                if (!String.IsNullOrEmpty(where))
                    where += (String.IsNullOrEmpty(appendWhere) ? String.Empty : " AND (" + appendWhere + ")");
                else
                    where = appendWhere;

                StringBuilder sb = new StringBuilder();
                sb.Append(filter.SubFieldsAndAlias);
                foreach (string fieldname in filter.SubFields.Split(' '))
                {
                    if (fieldname == "#SHAPE#")
                    {
                        if (sb.Length > 0) sb.Append(",");

                        sb.Append(_etcon.XFieldName + "," + _etcon.YFieldName);
                        _addShape = true;
                    }
                    if (fieldname == "*")
                        _addShape = true;
                }

                string fields = sb.ToString().Replace(",#SHAPE#", "").Replace("#SHAPE#,", "").Replace("#SHAPE#", "");

                //_tab = conn.Select(sb.ToString(), etconn.TableName, where);
                //_addShape = _tab.Columns.Contains(_etcon.XFieldName) &&
                //            _tab.Columns.Contains(_etcon.YFieldName);
                _dbReader = conn.DataReader("select " + fields + " from " + etconn.TableName + (String.IsNullOrEmpty(where) ? String.Empty : " WHERE " + where), out _dbConnection);
            }
        }

        #region IFeatureCursor Member

        public override IFeature NextFeature
        {
            get
            {
                //if (_etcon == null || _tab == null || _tab.Rows.Count <= _pos)
                //    return null;

                //DataRow row = _tab.Rows[_pos++];

                //Feature feature = new Feature();
                //if (_addShape)
                //    feature.Shape = new Point(Convert.ToDouble(row[_etcon.XFieldName]), Convert.ToDouble(row[_etcon.YFieldName]));

                //foreach (DataColumn col in _tab.Columns)
                //{
                //    if (col.ColumnName == _etcon.IdFieldName) 
                //        try
                //        {
                //            feature.OID = Convert.ToInt32(row[col.ColumnName]);
                //        }
                //        catch { }

                //    feature.Fields.Add(new FieldValue(col.ColumnName, row[col.ColumnName]));
                //}

                //base.Transform(feature);
                //return feature;

                if (_dbReader == null)
                    return null;
                try
                {
                    while (true)
                    {
                        if (!_dbReader.Read())
                            return null;

                        Feature feature = new Feature();
                        double x = 0.0, y = 0.0;
                        for (int i = 0; i < _dbReader.FieldCount; i++)
                        {
                            string name = _dbReader.GetName(i);
                            object obj = _dbReader.GetValue(i);
                            if (name == _etcon.XFieldName && obj != DBNull.Value)
                                x = Convert.ToDouble(obj);
                            else if (name == _etcon.YFieldName && obj != DBNull.Value)
                                y = Convert.ToDouble(obj);
                            else if (name == _etcon.IdFieldName && obj != DBNull.Value)
                            {
                                try
                                {
                                    feature.OID = Convert.ToInt32(obj);
                                }
                                catch { }
                            }

                            FieldValue fv = new FieldValue(name, obj);
                            feature.Fields.Add(fv);
                        }

                        if (_addShape)
                        {
                            feature.Shape = new Point(x, y);

                            if (_spatialFilter != null)
                            {
                                if (!gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, feature.Shape))
                                {
                                    continue;
                                }
                            }
                        }
                        Transform(feature);
                        return feature;
                    }
                }
                catch (Exception ex)
                {
                    Dispose();
                    throw (ex);
                }
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            if (_dbReader != null)
            {
                _dbReader.Close();
                _dbReader = null;
            }
            if (_dbConnection != null)
            {
                if (_dbConnection.State == ConnectionState.Open) _dbConnection.Close();
                _dbConnection.Dispose();
                _dbConnection = null;
            }
        }

        #endregion
    }
}
