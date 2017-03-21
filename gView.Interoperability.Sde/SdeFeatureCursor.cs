using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using gView.SDEWrapper;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Interoperability.Sde
{
    internal class SdeFeatureCursor : FeatureCursor
    {
        private SdeDataset _dataset;
        private SdeQueryInfo _queryInfo;
        private ArcSdeConnection _connection;
        private string _errMsg = "";
        private List<IField> _queryFields = null;

        public SdeFeatureCursor(SdeDataset dataset, ITableClass tc, IQueryFilter filter)
            : base((tc is IFeatureClass) ? ((IFeatureClass)tc).SpatialReference : null,
                   (filter != null) ? filter.FeatureSpatialReference : null)
        {
            try
            {
                if (filter != null && filter.SubFields != "*") filter.AddField(tc.IDFieldName);
                filter.fieldPrefix = tc.Name + ".";
                filter.fieldPostfix = "";

                Int32 err_no = 0;

                _dataset = dataset;
                if (_dataset == null) return;

                //_connection = _dataset.AllocConnection();
                _connection = new ArcSdeConnection(dataset.ConnectionString);
                if (!_connection.Open()) return;

                _queryInfo = new SdeQueryInfo(_connection, tc, filter);
                if (_queryInfo.ErrorMessage != "")
                {
                    Dispose();
                    return;
                }

                //if (Wrapper92.SE_stream_create(_connection.SeConnection, ref _stream) != 0)
                //{
                //    Dispose();
                //    return;
                //}

                _connection.ResetStream();

                // SE_stream_set_state sollte auch aufgerufen werden (siehe mapsde.c von UMN)
                if (Wrapper92.SE_stream_set_state(
                    _connection.SeStream,
                    CONST.SE_DEFAULT_STATE_ID,
                    CONST.SE_DEFAULT_STATE_ID,
                    CONST.SE_STATE_DIFF_NOCHECK) != 0)
                {
                    Dispose();
                    return;
                }

                if ((err_no = Wrapper92.SE_stream_query_with_info(_connection.SeStream, _queryInfo.SeQueryInfo)) != 0)
                {
                    Dispose();
                    return;
                }

                if (_queryInfo.IsSpatial)
                {
                    SE_FILTER se_filter = _queryInfo.Filter_Shape;
                    if ((err_no = Wrapper92.SE_stream_set_spatial_constraints(_connection.SeStream, CONST.SE_SPATIAL_FIRST, false, 1, ref se_filter)) != 0)
                    {
                        _errMsg = Wrapper92.GetErrorMsg(_connection.SeConnection, err_no);
                        Dispose();
                        return;
                    }
                }
                else
                {
                    /*
                    SE_FILTER se_filter = _queryInfo.Filter_Id;
                    if (Wrapper92.SE_stream_set_spatial_constraints(_stream, CONST.SE_SPATIAL_FIRST, false, 1, ref se_filter) != 0)
                    {
                        Release();
                        return;
                    }
                     * */
                }

                if (Wrapper92.SE_stream_execute(_connection.SeStream) != 0)
                {
                    Dispose();
                    return;
                }

                _queryFields = _queryInfo.QueryFields;
                _queryInfo.Dispose();
                _queryInfo = null;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message + "\n" + ex.StackTrace;
                Dispose();
            }
        }

        #region IFeatureCursor Member

        int i = 0;
        public override IFeature NextFeature
        {
            get
            {
                if (_connection == null) return null;

                Int32 err_no;
                err_no = Wrapper92.SE_stream_fetch(_connection.SeStream);
                if (err_no == CONST.SE_FINISHED)
                {
                    Dispose();
                    return null;
                }
                if (err_no != 0)
                {
                    _errMsg = Wrapper92.GetErrorMsg(_connection.SeConnection, err_no);
                    Dispose();
                    return null;
                }
                i++;
                IFeature feature = FetchNextFeature();
                Transform(feature);
                return feature;
            }
        }

        #endregion

        #region ICursor Member

        public override void Dispose()
        {
            base.Dispose();
            if (_queryInfo != null)
            {
                _queryInfo.Dispose();
                _queryInfo = null;
            }

            if (_connection != null)
            {
                try
                {
                    _connection.Close();
                }
                catch { }
                _connection = null;
            }
        }

        #endregion

        private IFeature FetchNextFeature()
        {
            try
            {
                Feature feat = new Feature();

                System.Int16 index = 1;
                foreach (IField field in _queryFields)
                {
                    switch (field.type)
                    {
                        case FieldType.ID:
                            feat.OID = FetchInteger(index);
                            feat.Fields.Add(new FieldValue(field.name, feat.OID));
                            break;
                        case FieldType.Shape:
                            feat.Shape = FetchShape(index);
                            break;
                        case FieldType.boolean:
                            feat.Fields.Add(new FieldValue(field.name, "???"));
                            break;
                        case FieldType.character:
                            feat.Fields.Add(new FieldValue(field.name, "???"));
                            break;
                        case FieldType.Date:
                            feat.Fields.Add(new FieldValue(field.name, FetchDate(index)));
                            break;
                        case FieldType.Double:
                            feat.Fields.Add(new FieldValue(field.name, FetchDouble(index)));
                            break;
                        case FieldType.Float:
                            feat.Fields.Add(new FieldValue(field.name, FetchFloat(index)));
                            break;
                        case FieldType.biginteger:
                        case FieldType.integer:
                            feat.Fields.Add(new FieldValue(field.name, FetchInteger(index)));
                            break;
                        case FieldType.smallinteger:
                            feat.Fields.Add(new FieldValue(field.name, FetchSmallInteger(index)));
                            break;
                        case FieldType.String:
                            feat.Fields.Add(new FieldValue(field.name, FetchString(index, field.size)));
                            break;
                        case FieldType.NString:
                            feat.Fields.Add(new FieldValue(field.name, FetchNString(index, field.size)));
                            break;
                    }
                    index++;
                }

                return feat;
            }
            catch
            {
                return null;
            }
        }

        private int FetchInteger(System.Int16 index)
        {
            System.Int32 val = 0;
            System.Int32 err_no = Wrapper92.SE_stream_get_integer(_connection.SeStream, index, ref val);

            if (err_no == 0) return val;

            return 0;
        }

        private short FetchSmallInteger(System.Int16 index)
        {
            System.Int16 val = 0;
            System.Int32 err_no = Wrapper92.SE_stream_get_smallint(_connection.SeStream, index, ref val);

            if (err_no == 0) return val;

            return 0;
        }

        private double FetchDouble(System.Int16 index)
        {
            System.Double val = 0;
            System.Int32 err_no = Wrapper92.SE_stream_get_double(_connection.SeStream, index, ref val);

            if (err_no == 0) return val;

            return 0;
        }

        private float FetchFloat(System.Int16 index)
        {
            float val = 0;
            System.Int32 err_no = Wrapper92.SE_stream_get_float(_connection.SeStream, index, ref val);

            if (err_no == 0) return val;

            return 0;
        }

        private string FetchString(System.Int16 index, int size)
        {
            try
            {
                byte[] buffer = new byte[size + 1];
                System.Int32 err_no = Wrapper92.SE_stream_get_string(_connection.SeStream, index, buffer);

                if (err_no == -1004) return String.Empty; //return null;
                if (err_no != 0)
                    return "<ERROR>:" + Wrapper92.GetErrorMsg(_connection.SeConnection, err_no);

                return System.Text.Encoding.UTF7.GetString(buffer).Replace("\0", "");
            }
            catch (Exception ex) { return "<EXCEPTION>:" + ex.Message; }
        }

        private string FetchNString(System.Int16 index, int size)
        {
            try
            {
                byte[] buffer = new byte[(size + 1) * 2];
                System.Int32 err_no = Wrapper92.SE_stream_get_nstring(_connection.SeStream, index, buffer);

                if (err_no == -1004) return String.Empty; //return null;
                if (err_no != 0)
                    return "<ERROR>:" + Wrapper92.GetErrorMsg(_connection.SeConnection, err_no);

                return System.Text.Encoding.Unicode.GetString(buffer).Replace("\0", "");
            }
            catch (Exception ex) { return "<EXCEPTION>:" + ex.Message; }
        }

        private DateTime? FetchDate(System.Int16 index)
        {
            tm TM = new tm();
            System.Int32 err_no = Wrapper92.SE_stream_get_date(_connection.SeStream, index, ref TM);

            if (err_no == -1004) return null;
            if (err_no != 0)
                return new DateTime(1, 1, 1);

            return new DateTime(
                TM.tm_year + 1900,
                TM.tm_mon + 1,
                TM.tm_mday);
        }
        private IGeometry FetchShape(System.Int16 index)
        {
            unsafe
            {
                System.Int32 err_no = 0;
                SE_SHAPE shape_val = new SE_SHAPE();

                System.Int32* part_offsets = null;
                System.Int32* subp_offsets = null;
                SE_POINT* points = null;
                try
                {
                    err_no = Wrapper92.SE_shape_create(new SE_COORDREF(), ref shape_val);
                    if (err_no != 0) return null;

                    err_no = Wrapper92.SE_stream_get_shape(_connection.SeStream, index, shape_val);
                    if (err_no != 0) return null;

                    Int32 shapeType = 0, numPoints = 0, numParts = 0, numSubparts = 0;

                    err_no = Wrapper92.SE_shape_get_type(shape_val, ref shapeType);
                    if (err_no != 0 || shapeType == CONST.SG_NIL_SHAPE) return null;
                    err_no = Wrapper92.SE_shape_get_num_points(shape_val, 0, 0, ref numPoints);
                    if (err_no != 0) return null;
                    err_no = Wrapper92.SE_shape_get_num_parts(shape_val, ref numParts, ref numSubparts);
                    if (err_no != 0) return null;

                    part_offsets = (System.Int32*)Marshal.AllocHGlobal((numParts + 1) * sizeof(System.Int32));
                    subp_offsets = (System.Int32*)Marshal.AllocHGlobal((numSubparts + 1) * sizeof(System.Int32));
                    points = (SE_POINT*)Marshal.AllocHGlobal(numPoints * sizeof(SE_POINT));

                    part_offsets[numParts] = numSubparts;
                    subp_offsets[numSubparts] = numPoints;

                    err_no = Wrapper92.SE_shape_get_all_points(
                         shape_val,
                         SE_ROTATION_TYPE.SE_DEFAULT_ROTATION,
                         (IntPtr)part_offsets,
                         (IntPtr)subp_offsets,
                         (IntPtr)points,
                         (IntPtr)null,
                         (IntPtr)null);
                    if (err_no != 0) return null;

                    IGeometry ret = null;
                    switch (shapeType)
                    {
                        case CONST.SG_POINT_SHAPE:
                            if (numPoints == 1)
                            {
                                ret = new Point(points[0].x, points[0].y);
                            }
                            else if (numPoints > 1)
                            {
                                MultiPoint mPoint_ = new MultiPoint();
                                for (int i = 0; i < numPoints; i++)
                                    mPoint_.AddPoint(new Point(points[i].x, points[i].y));
                                ret = mPoint_;
                            }
                            break;
                        case CONST.SG_MULTI_POINT_SHAPE:
                            MultiPoint mPoint = new MultiPoint();
                            for (int i = 0; i < numPoints; i++)
                                mPoint.AddPoint(new Point(points[i].x, points[i].y));
                            ret = mPoint;
                            break;
                        case CONST.SG_LINE_SHAPE:
                        case CONST.SG_SIMPLE_LINE_SHAPE:
                        case CONST.SG_MULTI_LINE_SHAPE:
                        case CONST.SG_MULTI_SIMPLE_LINE_SHAPE:
                            Polyline polyline = new Polyline();
                            for (int s = 0; s < numSubparts; s++)
                            {
                                Path path = new Path();
                                int to = subp_offsets[s + 1];
                                for (int i = subp_offsets[s]; i < to; i++)
                                {
                                    path.AddPoint(new Point(points[i].x, points[i].y));
                                }
                                polyline.AddPath(path);
                            }
                            ret = polyline;
                            break;
                        case CONST.SG_AREA_SHAPE:
                        case CONST.SG_MULTI_AREA_SHAPE:
                            Polygon polygon = new Polygon();
                            for (int s = 0; s < numSubparts; s++)
                            {
                                Ring ring = new Ring();
                                int to = subp_offsets[s + 1];
                                for (int i = subp_offsets[s]; i < to; i++)
                                {
                                    ring.AddPoint(new Point(points[i].x, points[i].y));
                                }
                                polygon.AddRing(ring);
                            }
                            ret = polygon;
                            break;
                    }
                    return ret;
                }
                catch
                {
                    return null;
                }
                finally
                {
                    if (part_offsets != null) Marshal.FreeHGlobal((System.IntPtr)part_offsets);
                    if (subp_offsets != null) Marshal.FreeHGlobal((System.IntPtr)subp_offsets);
                    if (points != null) Marshal.FreeHGlobal((System.IntPtr)points);

                    if (shape_val.handle != 0)
                        Wrapper92.SE_shape_free(shape_val);
                }
            }
        }
    }
}
