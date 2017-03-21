using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace gView.SDEWrapper
{
    public class CONST
    {
        public const int SE_MAX_MESSAGE_LENGTH = 512;
        public const int SE_MAX_SQL_MESSAGE_LENGTH = 4096;
        public const int SE_MAX_CONFIG_KEYWORD_LEN = 32;
        public const int SE_MAX_DESCRIPTION_LEN = 64;
        public const int SE_MAX_DATABASE_LEN = 32;
        public const int SE_MAX_OWNER_LEN = 32;
        public const int SE_MAX_TABLE_LEN = 160;
        public const int SE_MAX_COLUMN_LEN = 32;

        public const int SE_QUALIFIED_TABLE_NAME = (SE_MAX_DATABASE_LEN + SE_MAX_OWNER_LEN + SE_MAX_TABLE_LEN + 2);

        public const System.Int32 SE_NIL_TYPE_MASK = (1);
        public const System.Int32 SE_POINT_TYPE_MASK = (1 << 1);
        public const System.Int32 SE_LINE_TYPE_MASK = (1 << 2);
        public const System.Int32 SE_SIMPLE_LINE_TYPE_MASK = (1 << 3);
        public const System.Int32 SE_AREA_TYPE_MASK = (1 << 4);
        public const System.Int32 SE_UNVERIFIED_SHAPE_MASK = (1 << 11);
        public const System.Int32 SE_MULTIPART_TYPE_MASK = (1 << 18);


        public const System.Int32 SE_SMALLINT_TYPE = 1;   /* 2-byte Integer */
        public const System.Int32 SE_INTEGER_TYPE = 2;   /* 4-byte Integer */
        public const System.Int32 SE_FLOAT_TYPE = 3;   /* 4-byte Float */
        public const System.Int32 SE_DOUBLE_TYPE = 4;   /* 8-byte Float */
        public const System.Int32 SE_STRING_TYPE = 5;   /* Null Term. Character Array */
        public const System.Int32 SE_BLOB_TYPE = 6;   /* Variable Length Data */
        public const System.Int32 SE_DATE_TYPE = 7;   /* Struct tm Date */
        public const System.Int32 SE_SHAPE_TYPE = 8;   /* Shape geometry (SE_SHAPE) */
        public const System.Int32 SE_RASTER_TYPE = 9;   /* Raster */
        public const System.Int32 SE_XML_TYPE = 10;  /* XML Document */
        public const System.Int32 SE_INT64_TYPE = 11;  /* 8-byte Integer */
        public const System.Int32 SE_UUID_TYPE = 12;  /* A Universal Unique ID */
        public const System.Int32 SE_CLOB_TYPE = 13;  /* Character variable length data */
        public const System.Int32 SE_NSTRING_TYPE = 14;  /* UNICODE Null Term. Character Array */
        public const System.Int32 SE_NCLOB_TYPE = 15;  /* UNICODE Character Large Object */

        public const System.Int32 SE_POINT_TYPE = 20;  /* Point ADT */
        public const System.Int32 SE_CURVE_TYPE = 21;  /* LineString ADT */
        public const System.Int32 SE_LINESTRING_TYPE = 22;  /* LineString ADT */
        public const System.Int32 SE_SURFACE_TYPE = 23;  /* Polygon ADT */
        public const System.Int32 SE_POLYGON_TYPE = 24;  /* Polygon ADT */
        public const System.Int32 SE_GEOMETRYCOLLECTION_TYPE = 25;  /* MultiPoint ADT */
        public const System.Int32 SE_MULTISURFACE_TYPE = 26;  /* LineString ADT */
        public const System.Int32 SE_MULTICURVE_TYPE = 27;  /* LineString ADT */
        public const System.Int32 SE_MULTIPOINT_TYPE = 28;  /* MultiPoint ADT */
        public const System.Int32 SE_MULTILINESTRING_TYPE = 29;  /* MultiLineString ADT */
        public const System.Int32 SE_MULTIPOLYGON_TYPE = 30;  /* MultiPolygon ADT */
        public const System.Int32 SE_GEOMETRY_TYPE = 31;  /* Geometry ADT */


        public const System.Int32 SE_QUERYTYPE_ATTRIBUTE_FIRST = 1;
        public const System.Int32 SE_QUERYTYPE_JFA = 2;
        public const System.Int32 SE_QUERYTYPE_JSF = 3;
        public const System.Int32 SE_QUERYTYPE_JSFA = 4;
        public const System.Int32 SE_QUERYTYPE_V3 = 5;
        public const System.Int32 SE_MAX_QUERYTYPE = 5;

        /************************************************************
        *** SEARCH ORDERS
        ************************************************************/
        public const System.Int16 SE_ATTRIBUTE_FIRST = 1;   /* DO NOT USE SPATIAL INDEX */
        public const System.Int16 SE_SPATIAL_FIRST = 2;   /* USE SPATIAL INDEX */
        public const System.Int16 SE_OPTIMIZE = 3;

        /*
         * ...Search Methods...
         */
        public const System.Int32 SM_ENVP = 0;   /* ENVELOPES OVERLAP */
        public const System.Int32 SM_ENVP_BY_GRID = 1;   /* ENVELOPES OVERLAP */
        public const System.Int32 SM_CP = 2;   /* COMMON POINT */
        public const System.Int32 SM_LCROSS = 3;   /* LINE CROSS */
        public const System.Int32 SM_COMMON = 4;   /* COMMON EDGE/LINE */
        public const System.Int32 SM_CP_OR_LCROSS = 5;   /* COMMON POINT OR LINE CROSS */
        public const System.Int32 SM_LCROSS_OR_CP = 5;   /* COMMON POINT OR LINE CROSS */
        public const System.Int32 SM_ET_OR_AI = 6;   /* EDGE TOUCH OR AREA INTERSECT */
        public const System.Int32 SM_AI_OR_ET = 6;   /* EDGE TOUCH OR AREA INTERSECT */
        public const System.Int32 SM_ET_OR_II = 6;   /* EDGE TOUCH OR INTERIOR INTERSECT */
        public const System.Int32 SM_II_OR_ET = 6;   /* EDGE TOUCH OR INTERIOR INTERSECT */
        public const System.Int32 SM_AI = 7;   /* AREA INTERSECT */
        public const System.Int32 SM_II = 7;   /* INTERIOR INTERSECT */
        public const System.Int32 SM_AI_NO_ET = 8;   /* AREA INTERSECT AND NO EDGE TOUCH */
        public const System.Int32 SM_II_NO_ET = 8;   /* INTERIOR INTERSECT AND NO EDGE TOUCH */
        public const System.Int32 SM_PC = 9;   /* PRIMARY CONTAINED IN SECONDARY */
        public const System.Int32 SM_SC = 10;  /* SECONDARY CONTAINED IN PRIMARY */
        public const System.Int32 SM_PC_NO_ET = 11;  /* PRIM CONTAINED AND NO EDGE TOUCH */
        public const System.Int32 SM_SC_NO_ET = 12;  /* SEC CONTAINED AND NO EDGE TOUCH */
        public const System.Int32 SM_PIP = 13;  /* FIRST POINT IN PRIMARY IN SEC */
        public const System.Int32 SM_IDENTICAL = 15;  /* IDENTICAL */
        public const System.Int32 SM_CBM = 16;	/* Calculus-based method [Clementini] */

        /********************************************************************
        *** SPATIAL FILTER TYPES FOR SPATIAL CONSTRAINTS AND STABLE SEARCHES
        *********************************************************************/
        public const System.Int32 SE_SHAPE_FILTER = 1;
        public const System.Int32 SE_ID_FILTER = 2;

        public const System.Int32 SE_FINISHED = -4;

        /*
        * ...Allowable shape types...
        */
        public const System.Int32 SG_NIL_SHAPE = 0;
        public const System.Int32 SG_POINT_SHAPE = 1;
        public const System.Int32 SG_LINE_SHAPE = 2;
        public const System.Int32 SG_SIMPLE_LINE_SHAPE = 4;
        public const System.Int32 SG_AREA_SHAPE = 8;
        public const System.Int32 SG_SHAPE_CLASS_MASK = 255;  /* Mask all of the previous */
        public const System.Int32 SG_SHAPE_MULTI_PART_MASK = 256;  /* Bit flag indicates mult parts */
        public const System.Int32 SG_MULTI_POINT_SHAPE = 257;
        public const System.Int32 SG_MULTI_LINE_SHAPE = 258;
        public const System.Int32 SG_MULTI_SIMPLE_LINE_SHAPE = 260;
        public const System.Int32 SG_MULTI_AREA_SHAPE = 264;

        /****************************/
        /***  State Reserved Ids  ***/
        /****************************/
        public const System.Int32 SE_BASE_STATE_ID = (0);
        public const System.Int32 SE_NULL_STATE_ID = (-1);
        public const System.Int32 SE_DEFAULT_STATE_ID = (-2);

        /*************************************/
        /***  State Conflict Filter Types  ***/
        /*************************************/
        public const System.Int32 SE_STATE_DIFF_NOCHECK = 0;
        public const System.Int32 SE_STATE_DIFF_NOCHANGE_UPDATE = 1;
        public const System.Int32 SE_STATE_DIFF_NOCHANGE_DELETE = 2;
        public const System.Int32 SE_STATE_DIFF_UPDATE_NOCHANGE = 3;
        public const System.Int32 SE_STATE_DIFF_UPDATE_UPDATE = 4;
        public const System.Int32 SE_STATE_DIFF_UPDATE_DELETE = 5;
        public const System.Int32 SE_STATE_DIFF_INSERT = 6;
    }

    public enum SE_ROTATION_TYPE
    {
        SE_DEFAULT_ROTATION,
        SE_LEFT_HAND_ROTATION,
        SE_RIGHT_HAND_ROTATION
    };

    // SE_ERROR
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_ERROR
    {
        public System.Int32 sde_error;
        public System.Int32 ext_error;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] err_msg1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] err_msg2;
    }

    // SE_CONNECTION
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_CONNECTION
    {
        public System.Int32 handle;
    };

    // SE_COLUMN_DEF
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_COLUMN_DEF
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONST.SE_MAX_COLUMN_LEN)]
        public byte[] column_name;                          /* the column name */
        public System.Int32 sde_type;                       /* the SDE data type */
        public System.Int32 size;                           /* the size of the column values */
        public System.Int16 decimal_digits;                 /* number of digits after decimal */
        public System.Boolean nulls_allowed;                /* allow NULL values ? */
        public System.Int16 row_id_type;                    /* column's use as table's row id */
    };

    // SE_POINT
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct SE_POINT
    {
        public double x, y;
    }

    // SE_REGINFO
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_REGINFO
    {
        public System.Int32 handle;
    };

    // SE_LAYERINFO
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_LAYERINFO
    {
        public System.Int32 handle;
    };

    // SE_ENVELOPE
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_ENVELOPE
    {
        public double minx;
        public double miny;
        public double maxx;
        public double maxy;
    };

    // SE_SHAPE
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_SHAPE
    {
        public System.Int32 handle;
    };

    // SE_QUERYINFO
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_QUERYINFO
    {
        public System.Int32 handle;
    };

    // SE_COORDREF
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_COORDREF
    {
        public System.Int32 handle;
    };


    //[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    //public struct SE_FILTER2
    //{
    //    [FieldOffset(0), MarshalAs(UnmanagedType.ByValTStr, SizeConst = CONST.SE_QUALIFIED_TABLE_NAME)]
    //    public string table;    /* the spatial table name */
    //    [FieldOffset(226), MarshalAs(UnmanagedType.ByValTStr, SizeConst = CONST.SE_MAX_COLUMN_LEN)]
    //    public string column;     /* the spatial column name */
    //    [FieldOffset(258)]
    //    public System.Int32 filter_type;                       /* the type of spatial filter */
    //    //union
    //    //{
    //    [FieldOffset(262)]
    //    public SE_SHAPE shape;                      /* a shape object */
    //    //struct id
    //    //{
    //    [FieldOffset(262)]
    //    public System.Int32 ID;                             /* A SDE_ROW_ID id for a shape */
    //    [FieldOffset(266), MarshalAs(UnmanagedType.ByValTStr, SizeConst = CONST.SE_QUALIFIED_TABLE_NAME)]
    //    string tableID;                                /* The shape's spatial table */
    //    //};
    //    //} filter;

    //    [FieldOffset(492)]
    //    public System.Int32 method;       /* the search method to satisfy */
    //    [FieldOffset(496)]
    //    public System.Boolean truth;                         /* TRUE to pass the test, FALSE if it must NOT pass */
    //    [FieldOffset(497)]
    //    public System.IntPtr cbm_source;                     /* set ONLY if the method is SM_CBM */
    //    [FieldOffset(401)]
    //    public System.IntPtr cbm_object_code;                /* internal system use only */
    //};

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_FILTER
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CONST.SE_QUALIFIED_TABLE_NAME)]
        public string table;    /* the spatial table name */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CONST.SE_MAX_COLUMN_LEN)]
        public string column;     /* the spatial column name */
        public System.Int32 filter_type;                       /* the type of spatial filter */
        //union
        //{
        public SE_SHAPE shape;                      /* a shape object */
        //struct id
        //{
        //public System.Int32 ID;                             /* A SDE_ROW_ID id for a shape */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CONST.SE_QUALIFIED_TABLE_NAME)]
        string tableID;                                /* The shape's spatial table */
        //};
        //} filter;

        public System.Int32 method;       /* the search method to satisfy */
        public System.Boolean truth;                         /* TRUE to pass the test, FALSE if it must NOT pass */
        public System.IntPtr cbm_source;                     /* set ONLY if the method is SM_CBM */
        public System.IntPtr cbm_object_code;                /* internal system use only */
    };

    // SE_STREAM
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_STREAM
    {
        public System.Int32 handle;
    };

    // tm
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct tm
    {
        public int tm_sec;     /* seconds after the minute - [0,59] */
        public int tm_min;     /* minutes after the hour - [0,59] */
        public int tm_hour;    /* hours since midnight - [0,23] */
        public int tm_mday;    /* day of the month - [1,31] */
        public int tm_mon;     /* months since January - [0,11] */
        public int tm_year;    /* years since 1900 */
        public int tm_wday;    /* days since Sunday - [0,6] */
        public int tm_yday;    /* days since January 1 - [0,365] */
        public int tm_isdst;   /* daylight savings time flag */
    };
}
