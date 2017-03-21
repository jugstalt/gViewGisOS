using System;
using System.Collections.Generic;
using System.Text;
using System.Text;
using System.Runtime.InteropServices;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.SDEWrapper.a10;

namespace gView.Interoperability.Sde.a10
{
    internal class SdeQueryInfo : IDisposable
    {
        private SE_QUERYINFO _queryInfo = new SE_QUERYINFO();
        private SE_SHAPE _shape = new SE_SHAPE();
        private System.Int32 _err_no = 0;
        private string _errMsg = "";
        private SE_FILTER _seFilter = new SE_FILTER();
        private bool _isSpatial = false;
        private List<IField> _queryFields = new List<IField>();

        public SdeQueryInfo(ArcSdeConnection connection, ITableClass tc, IQueryFilter filter)
        {
            if (tc == null) return;

            try
            {
                if (filter is ISpatialFilter && ((ISpatialFilter)filter).Geometry != null && tc is IFeatureClass && tc.Dataset is SdeDataset)
                {
                    SE_ENVELOPE maxExtent = new SE_ENVELOPE();
                    SE_COORDREF coordRef = ((SdeDataset)tc.Dataset).GetSeCoordRef(connection, tc.Name, ((IFeatureClass)tc).ShapeFieldName, ref maxExtent);
                    if (((SdeDataset)tc.Dataset).lastErrorMsg != "") return;

                    _isSpatial = true;
                    _err_no = Wrapper10.SE_shape_create(coordRef, ref _shape);
                    ((SdeDataset)tc.Dataset).FreeSeCoordRef(coordRef);
                    if (_err_no != 0) return;

                    //IEnvelope env = ((ISpatialFilter)filter).Geometry.Envelope;
                    //SE_ENVELOPE seEnvelope = new SE_ENVELOPE();

                    //seEnvelope.minx = Math.Max(env.minx, maxExtent.minx);
                    //seEnvelope.miny = Math.Max(env.miny, maxExtent.miny);
                    //seEnvelope.maxx = Math.Min(env.maxx, maxExtent.maxx);
                    //seEnvelope.maxy = Math.Min(env.maxy, maxExtent.maxy);

                    //if (seEnvelope.minx == seEnvelope.maxx && seEnvelope.miny == seEnvelope.maxy)
                    //{
                    //    /* fudge a rectangle so we have a valid one for generate_rectangle */
                    //    /* FIXME: use the real shape for the query and set the filter_type 
                    //       to be an appropriate type */
                    //    seEnvelope.minx = seEnvelope.minx - 0.001;
                    //    seEnvelope.maxx = seEnvelope.maxx + 0.001;
                    //    seEnvelope.miny = seEnvelope.miny - 0.001;
                    //    seEnvelope.maxy = seEnvelope.maxy + 0.001;
                    //}

                    //_err_no = Wrapper10.SE_shape_generate_rectangle(ref seEnvelope, _shape);
                    _err_no = gView.SDEWrapper.a10.Functions.SE_GenerateGeometry(_shape, ((ISpatialFilter)filter).Geometry, maxExtent);
                    if (_err_no != 0) 
                        return;

                    _seFilter.shape = _shape;

                    /* set spatial constraint column and table */
                    _seFilter.table = tc.Name.PadRight(CONST.SE_QUALIFIED_TABLE_NAME, '\0'); ;
                    _seFilter.column = ((IFeatureClass)tc).ShapeFieldName.PadRight(CONST.SE_MAX_COLUMN_LEN, '\0');

                    /* set a couple of other spatial constraint properties */

                    switch (((ISpatialFilter)filter).SpatialRelation)
                    {
                        /*case spatialRelation.SpatialRelationMapEnvelopeIntersects:
                            _seFilter.method = CONST.SM_ENVP_BY_GRID;
                            break;
                         * */
                        case spatialRelation.SpatialRelationEnvelopeIntersects:
                            _seFilter.method = CONST.SM_ENVP;
                            break;
                        default:
                            _seFilter.method = CONST.SM_AI;
                            break;
                    }

                    
                    _seFilter.filter_type = CONST.SE_SHAPE_FILTER;
                    _seFilter.truth = true;  // True;
                }

                _err_no = Wrapper10.SE_queryinfo_create(ref _queryInfo);
                if (_err_no != 0) return;

                _err_no = Wrapper10.SE_queryinfo_set_tables(_queryInfo, 1, new string[] { tc.Name }, null);
                if (_err_no != 0) return;

                string [] fields;
                if (filter.SubFields == "" || filter.SubFields.Contains("*") || filter.SubFields == null)
                {
                    StringBuilder subFields = new StringBuilder();
                    foreach (IField field in tc.Fields)
                    {
                        if (subFields.Length != 0) subFields.Append(" ");
                        subFields.Append(tc.Name + "." + field.name);
                        _queryFields.Add(field);
                    }
                    fields = subFields.ToString().Split(' ');
                }
                else
                {
                    fields = filter.SubFields.Split(' ');
                    foreach (string fieldname in fields)
                    {
                        string fname = fieldname;
                        if (fieldname.ToLower().IndexOf("distinct(") == 0)
                        {
                            fname = fieldname.Substring(9, fieldname.IndexOf(")") - 9);
                        }

                        IField field = tc.FindField(fname);
                        if (field == null)
                        {
                            if (filter.IgnoreUndefinedFields)
                                continue;

                            _errMsg = "Can't get Field " + fname;
                            Cleanup();
                            return;
                        }
                        _queryFields.Add(field);
                    }
                }

                _err_no = Wrapper10.SE_queryinfo_set_columns(_queryInfo, fields.Length, fields);
                if (_err_no != 0) return;

                string where = "";
                if (filter != null)
                {
                    if (filter is IRowIDFilter)
                    {
                        where = ((IRowIDFilter)filter).RowIDWhereClause;
                    }
                    else
                    {
                        where = filter.WhereClause;
                    }
                }
                if (where != "")
                {
                    _err_no = Wrapper10.SE_queryinfo_set_where_clause(_queryInfo, where);
                    if (_err_no != 0) return;
                }

                _err_no = Wrapper10.SE_queryinfo_set_query_type(_queryInfo, CONST.SE_QUERYTYPE_JSFA);
                if (_err_no != 0) return;


            }
            catch (Exception ex)
            {
                _errMsg = "SeQueryInfo:" + ex.Message + "\n" + ex.StackTrace;
                _err_no = -1;
            }
            finally
            {
                if (_err_no != 0)
                {
                    _errMsg = Wrapper10.GetErrorMsg(new SE_CONNECTION(), _err_no);
                    Cleanup();
                }
            }
        }

        private void Cleanup()
        {
            if (_shape.handle != IntPtr.Zero)
            {
                Wrapper10.SE_shape_free(_shape);
                _shape.handle = IntPtr.Zero;
            }
            if (_queryInfo.handle != IntPtr.Zero)
            {
                Wrapper10.SE_queryinfo_free(_queryInfo);
                _queryInfo.handle = IntPtr.Zero;
            }
        }

        public string ErrorMessage
        {
            get { return _errMsg; }
        }

        public SE_QUERYINFO SeQueryInfo
        {
            get { return _queryInfo; }
        }
        public bool IsSpatial
        {
            get { return _isSpatial; }
        }

        public SE_FILTER Filter_Shape
        {
            get { return _seFilter; }
        }
        public SE_FILTER Filter_Id
        {
            get { return _seFilter; }
        }
        public List<IField> QueryFields
        {
            get { return _queryFields; }
        }

        #region IDisposable Member

        public void Dispose()
        {
            Cleanup();
        }

        #endregion
    }
}
