using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace gView.SDEWrapper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_ERROR_64
    {
        public System.Int64 sde_error;
        public System.Int64 ext_error;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] err_msg1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] err_msg2;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_CONNECTION_64
    {
        public System.Int64 handle;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_COLUMN_DEF_64
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONST.SE_MAX_COLUMN_LEN)]
        public byte[] column_name;                          /* the column name */
        public System.Int32 sde_type;                       /* the SDE data type */
        public System.Int32 size;                           /* the size of the column values */
        public System.Int16 decimal_digits;                 /* number of digits after decimal */
        public System.Boolean nulls_allowed;                /* allow NULL values ? */
        public System.Int16 row_id_type;                    /* column's use as table's row id */
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_REGINFO_64
    {
        public System.Int64 handle;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_LAYERINFO_64
    {
        public System.Int64 handle;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_QUERYINFO_64
    {
        public System.Int64 handle;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_COORDREF_64
    {
        public System.Int64 handle;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_STREAM_64
    {
        public System.Int64 handle;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SE_SHAPE_64
    {
        public System.Int64 handle;
    };
}
