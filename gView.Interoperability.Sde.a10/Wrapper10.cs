using System;
using System.Text;
using System.Runtime.InteropServices;

namespace gView.SDEWrapper.a10
{
    public class Wrapper10
    {
        #region Connection
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_connection_create(string server, string instance, string database, string username, string password, ref SE_ERROR error, ref SE_CONNECTION conn);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_connection_free(SE_CONNECTION conn);
        #endregion

        #region Errorhandling
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_connection_get_error_string(SE_CONNECTION connection, System.Int32 error_code, [MarshalAs(UnmanagedType.LPArray)] byte[] error_string);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_connection_get_ext_error(SE_CONNECTION connection, ref SE_ERROR error);

        public static string GetErrorMsg(SE_CONNECTION connection, SE_ERROR error)
        {
            return GetErrorMsg(connection, error.sde_error);
        }
        public static string GetErrorMsg(SE_CONNECTION connection, Int32 err_no)
        {
            try
            {
                byte[] buffer = new byte[CONST.SE_MAX_MESSAGE_LENGTH];
                if (SE_connection_get_error_string(connection, err_no, buffer) == 0)
                {
                    return ASCIIEncoding.ASCII.GetString(buffer).Replace("\0", "");
                }
                else
                {
                    return "SDE ERROR: Unknown sde error!!";
                }
            }
            catch (Exception ex)
            {
                return "SDE ERROR: GetErrorMsg - " + ex.Message;
            }
        }

        public static SE_ERROR GetExtError(SE_CONNECTION connection)
        {
            SE_ERROR error = new SE_ERROR();

            Int32 err_no = SE_connection_get_ext_error(connection, ref error);
            return error;
        }
        #endregion

        #region Registration
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_registration_get_info_list(SE_CONNECTION connection, ref Pointer2Pointer reg_list_addr, ref System.Int32 count_addr);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_registration_get_info(SE_CONNECTION connection, string table, IntPtr reginfo);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_reginfo_has_layer(IntPtr registration);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_reginfo_get_table_name(IntPtr registration, [MarshalAs(UnmanagedType.LPArray)] byte[] table_name);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_reginfo_create(ref IntPtr registration);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void SE_reginfo_free(IntPtr registration);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_reginfo_get_rowid_column(IntPtr registration, [MarshalAs(UnmanagedType.LPArray)] byte[] rowid_column, ref System.Int32 type);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void SE_reginfo_get_description(IntPtr registration, [MarshalAs(UnmanagedType.LPArray)] byte[] description);
        #endregion

        #region Layer/LayerInfo
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_layer_get_info_list(SE_CONNECTION connection, ref IntPtr layer_list, ref System.Int32 count_addr);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void SE_layerinfo_free(SE_LAYERINFO layer);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_layerinfo_create(SE_COORDREF coordref, ref SE_LAYERINFO layer);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_layerinfo_get_shape_types(SE_LAYERINFO layerInfo, ref System.Int32 shape_types);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_layer_get_info(SE_CONNECTION connection, string table, string column, SE_LAYERINFO layer);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_layerinfo_get_spatial_column(SE_LAYERINFO layerInfo, [MarshalAs(UnmanagedType.LPArray)] byte[] table, [MarshalAs(UnmanagedType.LPArray)] byte[] column);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_layerinfo_get_envelope(SE_LAYERINFO layerInfo, ref SE_ENVELOPE envelope);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_layerinfo_get_coordref(SE_LAYERINFO layer, SE_COORDREF coordref);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void SE_layer_free_info_list(System.Int32 count, IntPtr layerinfo_list);
        #endregion

        #region Table

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_table_describe(SE_CONNECTION connection, string table, ref System.Int16 num_columns, ref IntPtr column_defs);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void SE_table_free_descriptions(IntPtr column_defs);

        #endregion

        #region Shape
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_create(SE_COORDREF coordref, ref SE_SHAPE shape);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void SE_shape_free(SE_SHAPE shape);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_generate_rectangle(ref SE_ENVELOPE rect, SE_SHAPE tgt_shape);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_get_all_points(SE_SHAPE shape, SE_ROTATION_TYPE rotation, IntPtr part_offsets, IntPtr subpart_offsets, IntPtr point_array, IntPtr z, IntPtr measure);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_get_type(SE_SHAPE shape, ref System.Int32 shape_type);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_get_num_parts(SE_SHAPE shape, ref System.Int32 num_partsref, ref System.Int32 num_subparts);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_get_num_points(SE_SHAPE shape, System.Int32 part_num, System.Int32 subpart_num, ref System.Int32 num_pts);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_generate_point(System.Int32 num_pts, IntPtr point_array, IntPtr z, IntPtr measure, SE_SHAPE tgt_shape);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_generate_line(System.Int32 num_pts, System.Int32 num_parts, IntPtr part_offsets, IntPtr point_array, IntPtr z, IntPtr measure, SE_SHAPE tgt_shape);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_shape_generate_polygon(System.Int32 num_pts, System.Int32 num_parts, IntPtr part_offsets, IntPtr point_array, IntPtr z, IntPtr measure, SE_SHAPE tgt_shape);

        #endregion

        #region Queryinfo
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_queryinfo_create(ref SE_QUERYINFO query_info);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_queryinfo_free(SE_QUERYINFO query_info);

        // Marshal für CHAR ** noch unklar; CHAR * ist string aber CHAR ** (Zeiger auf Stringarray)
        // MarshalAs(UnManagedType.LPArray, ArraySubType=UnManagedType.LPStr, SizeParamIndex=0)) 
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_queryinfo_set_tables(SE_QUERYINFO query_info, System.Int32 num_tables, [MarshalAs(UnmanagedType.LPArray)] string[] tables, [MarshalAs(UnmanagedType.LPArray)] string[] aliases);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_queryinfo_get_columns(SE_QUERYINFO query_info, ref System.Int32 num_columns, ref IntPtr columns);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_queryinfo_set_columns(SE_QUERYINFO query_info, System.Int32 num_columns, [MarshalAs(UnmanagedType.LPArray)] string[] columns);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_queryinfo_set_where_clause(SE_QUERYINFO query_info, string where_clause);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_queryinfo_set_query_type(SE_QUERYINFO query_info, System.Int32 query_type);
        #endregion

        #region Stream
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_create(SE_CONNECTION connection, ref SE_STREAM stream);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_free(SE_STREAM stream);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]  // reset 1..True
        public static extern System.Int32 SE_stream_close(SE_STREAM stream, System.Boolean reset);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_state(SE_STREAM stream, System.Int32 source_id, System.Int32 differences_id, System.Int32 difference_type);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_query_with_info(SE_STREAM stream, SE_QUERYINFO query_info);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_spatial_constraints(SE_STREAM stream, System.Int16 search_order, System.Boolean calc_masks, System.Int16 num_filters, ref SE_FILTER filters);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_execute(SE_STREAM stream);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_fetch(SE_STREAM stream);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_get_integer(SE_STREAM stream, System.Int16 column, ref System.Int32 int_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_get_shape(SE_STREAM stream, System.Int16 column, SE_SHAPE shape_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_get_smallint(SE_STREAM stream, System.Int16 column, ref System.Int16 short_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_get_double(SE_STREAM stream, System.Int32 column, ref System.Double double_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_get_float(SE_STREAM stream, System.Int16 column, ref float float_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_get_string(SE_STREAM stream, System.Int16 column, [MarshalAs(UnmanagedType.LPArray)] byte[] string_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_get_nstring(SE_STREAM stream, System.Int16 column, [MarshalAs(UnmanagedType.LPArray)] byte[] string_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_get_date(SE_STREAM stream, System.Int16 column, ref tm date_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_date(SE_STREAM stream, System.Int16 column, ref tm date_val);
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_date(SE_STREAM stream, System.Int16 column, IntPtr ptr);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_integer(SE_STREAM stream, System.Int16 column, ref System.Int32 int_val);
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_integer(SE_STREAM stream, System.Int16 column, IntPtr ptr);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_shape(SE_STREAM stream, System.Int16 column, SE_SHAPE shape_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_smallint(SE_STREAM stream, System.Int16 column, ref System.Int16 short_val);
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_smallint(SE_STREAM stream, System.Int16 column, IntPtr ptr);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_double(SE_STREAM stream, System.Int32 column, ref System.Double double_val);
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_double(SE_STREAM stream, System.Int32 column, IntPtr ptr);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_float(SE_STREAM stream, System.Int16 column, ref float float_val);
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_float(SE_STREAM stream, System.Int16 column, IntPtr ptr);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_string(SE_STREAM stream, System.Int16 column, [MarshalAs(UnmanagedType.LPStr)] string string_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_nstring(SE_STREAM stream, System.Int16 column, Byte[] string_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_set_uuid(SE_STREAM stream, System.Int16 column, [MarshalAs(UnmanagedType.LPStr)] string string_val);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_update_table(SE_STREAM stream, string table, System.Int16 num_columns, [MarshalAs(UnmanagedType.LPArray)] string[] columns, string where);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_insert_table(SE_STREAM stream, string table, System.Int16 num_columns, [MarshalAs(UnmanagedType.LPArray)] string[] columns);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_delete_row(SE_STREAM stream, string table, System.Int32 sde_row_id);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_stream_describe_column(SE_STREAM stream, System.Int32 column, ref SE_COLUMN_DEF column_def);
        #endregion

        #region Coordref
        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_coordref_create(ref SE_COORDREF coordref);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void SE_coordref_free(SE_COORDREF coordref);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_coordref_get_xy_envelope(SE_COORDREF coordref, ref SE_ENVELOPE extent);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_coordref_get_description(SE_COORDREF coordref, [MarshalAs(UnmanagedType.LPArray)] byte[] description);

        [DllImport("sde.dll", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern System.Int32 SE_coordref_get_srid(SE_COORDREF coordref, ref System.Int32 srid);
        #endregion
    }

}
