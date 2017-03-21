using System;
using System.Runtime.InteropServices;

namespace gView.Framework.Proj
{
	internal class NamespaceDoc 
	{
	}
	/// <summary>
	/// Zusammenfassung für Proj4Wrapper.
	/// </summary>
	
	public struct UV 
	{
		public double u,v;
	}

	public class MConst 
	{
		public const double PI=3.1415926535897932384626433832795;
		public const double DEG_2_RAD=0.017453292519943295769236907684886;
	}

	public class Proj4Wrapper
	{
        [DllImport("proj.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr pj_init(int argc, string[] args);

        [DllImport("proj.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr pj_init_plus(string args);

        [DllImport("proj.dll")]
        public static extern void pj_free(IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern UV pj_fwd(UV uv, IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern UV pj_inv(UV uv, IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern IntPtr pj_get_errno_ref();

        [DllImport("proj.dll")]
        public static extern IntPtr pj_strerrno(int errno);

        [DllImport("proj.dll")]
        public static extern int pj_is_latlong(IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern IntPtr pj_get_release();

        [DllImport("proj.dll")]
        public static extern int pj_transform(IntPtr src, IntPtr dst, int point_count, int point_offset, IntPtr x, IntPtr y, IntPtr z);

        [DllImport("proj.dll")]
        public static extern int pj_is_geocent(IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern void pj_set_searchpath(int count, IntPtr path);

        /*
        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
        public static extern int pj_init(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);

        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
        public static extern int pj_init_plus(IntPtr args);

        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
		public static extern string pj_get_def(int PJ);

        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
		public static extern UV pj_fwd(UV uv, int pj);

        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
		public static extern UV pj_inv(UV uv, int pj);

        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
		public static extern void pj_free(int pj);

        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
		public static extern bool pj_is_geocent(int pj);

        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
		public static extern bool pj_is_latlong(int pj);

        [System.Runtime.InteropServices.DllImport(@"proj.dll")]
        public static extern int pj_transform(int srcPJ, int dstPJ,
            int pointCount, int pointOffset,
            IntPtr x, IntPtr y, IntPtr z);
         * 
         * */

		/*
		// API
		[System.Runtime.InteropServices.DllImport(@"proj_api.dll")]
		public static extern int pjInit(int argc, string [] argv);

		[System.Runtime.InteropServices.DllImport(@"proj_api.dll")]
		public static extern int pjInitPlus(string args);

		[System.Runtime.InteropServices.DllImport(@"proj_api.dll")]
		public static extern UV pjFwd(UV uv, int pj);
		
		[System.Runtime.InteropServices.DllImport(@"proj_api.dll")]
		public static extern UV pjInv(UV uv, int pj);

		[System.Runtime.InteropServices.DllImport(@"proj_api.dll")]
		public static extern void pjFree(int pj);

		[System.Runtime.InteropServices.DllImport(@"proj_api.dll")]
		public static extern bool pjIsGeocent(int pj);

		[System.Runtime.InteropServices.DllImport(@"proj_api.dll")]
		public static extern int pjTransform(int srcPJ,int dstPJ,
			int pointCount,int pointOffset,
			double [] x,double [] y,double [] z);
		*/
	}
}
