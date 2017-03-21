using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace gView.DataSources.Raster
{
    internal class ImageFast
    {
        [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

        [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipLoadImageFromStream(UCOMIStream istream, out IntPtr image);

        [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdiplusStartup(out IntPtr token, ref StartupInput input, out StartupOutput output);

        [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdiplusShutdown(IntPtr token);

        [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipGetImageType(IntPtr image, out GdipImageTypeEnum type);

        [DllImport("ole32.dll")]
        static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out UCOMIStream ppstm);

        private static IntPtr gdipToken = IntPtr.Zero;

        static ImageFast()
        {
#if DEBUG
            //Console.WriteLine("Initializing GDI+");
#endif
            if (gdipToken == IntPtr.Zero)
            {
                StartupInput input = StartupInput.GetDefaultStartupInput();
                StartupOutput output;

                int status = GdiplusStartup(out gdipToken, ref input, out output);
#if DEBUG
                //if (status == 0)
                //    Console.WriteLine("Initializing GDI+ completed successfully");
#endif
                if (status == 0)
                    AppDomain.CurrentDomain.ProcessExit += new EventHandler(Cleanup_Gdiplus);
            }
        }

        private static void Cleanup_Gdiplus(object sender, EventArgs e)
        {
#if DEBUG
            //Console.WriteLine("GDI+ shutdown entered through ProcessExit event");
#endif
            if (gdipToken != IntPtr.Zero)
                GdiplusShutdown(gdipToken);

#if DEBUG
            //Console.WriteLine("GDI+ shutdown completed");
#endif
        }

        private static Type bmpType = typeof(System.Drawing.Bitmap);
        private static Type emfType = typeof(System.Drawing.Imaging.Metafile);

        public static Image FromFile(string filename)
        {
            filename = Path.GetFullPath(filename);
            IntPtr loadingImage = IntPtr.Zero;

            // We are not using ICM at all, fudge that, this should be FAAAAAST!
            if (GdipLoadImageFromFile(filename, out loadingImage) != 0)
            {
                throw new Exception("GDI+ threw a status error code.");
            }

            GdipImageTypeEnum imageType;
            if (GdipGetImageType(loadingImage, out imageType) != 0)
            {
                throw new Exception("GDI+ couldn't get the image type");
            }

            switch (imageType)
            {
                case GdipImageTypeEnum.Bitmap:
                    return (Bitmap)bmpType.InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { loadingImage });
                case GdipImageTypeEnum.Metafile:
                    return (Metafile)emfType.InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { loadingImage });
            }

            throw new Exception("Couldn't convert underlying GDI+ object to managed object");
        }

        public static Image FromStream(byte[] b)
        {
            IntPtr loadingImage = IntPtr.Zero;

            IntPtr nativePtr = Marshal.AllocHGlobal(b.Length);
            // copy byte array to native heap
            Marshal.Copy(b, 0, nativePtr, b.Length);
            // Create a UCOMIStream from the allocated memory
            UCOMIStream comStream;
            CreateStreamOnHGlobal(nativePtr, true, out comStream);

            // We are not using ICM at all, fudge that, this should be FAAAAAST!
            if (GdipLoadImageFromStream(comStream, out loadingImage) != 0)
            {
                //Marshal.FreeHGlobal(nativePtr);
                throw new Exception("GDI+ threw a status error code.");
            }
            //Marshal.FreeHGlobal(nativePtr);

            GdipImageTypeEnum imageType;
            if (GdipGetImageType(loadingImage, out imageType) != 0)
            {
                throw new Exception("GDI+ couldn't get the image type");
            }

            switch (imageType)
            {
                case GdipImageTypeEnum.Bitmap:
                    return (Bitmap)bmpType.InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { loadingImage });
                case GdipImageTypeEnum.Metafile:
                    return (Metafile)emfType.InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { loadingImage });
            }

            throw new Exception("Couldn't convert underlying GDI+ object to managed object");
        }

        private ImageFast() { }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupInput
    {
        public int GdiplusVersion;
        public IntPtr DebugEventCallback;
        public bool SuppressBackgroundThread;
        public bool SuppressExternalCodecs;

        public static StartupInput GetDefaultStartupInput()
        {
            StartupInput result = new StartupInput();
            result.GdiplusVersion = 1;
            result.SuppressBackgroundThread = false;
            result.SuppressExternalCodecs = false;
            return result;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupOutput
    {
        public IntPtr Hook;
        public IntPtr Unhook;
    }

    internal enum GdipImageTypeEnum
    {
        Unknown = 0,
        Bitmap = 1,
        Metafile = 2
    }

    // For framework 1.1 define the COM IStream interface (framework 2.x has it built in)
    // Thanks to Oliver Sturm for this
    [ComImport, Guid("0000000c-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStream
    {
        void Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pv, int cb, IntPtr pcbRead);
        void Write([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pv, int cb, IntPtr pcbWritten);
        void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition);
        void SetSize(long libNewSize);
        void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);
        void Commit(int grfCommitFlags);
        void Revert();
        void LockRegion(long libOffset, long cb, int dwLockType);
        void UnlockRegion(long libOffset, long cb, int dwLockType);
        void Stat(out STATSTG pstatstg, int grfStatFlag);
        void Clone(out IStream ppstm);
    }

    /// <summary>
 /// COM IStream wrapper for a MemoryStream.
 /// Thanks to Willy Denoyette for the if(System.IntPr != IntPtr.Zero) test for a NULL parameter via COM
 /// CLR will make the class implement the IDispatch COM interface
 /// so COM objects can make calls to IMemoryStream methods
 /// </summary>
 /*
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class IMemoryStream : MemoryStream, IStream
    {
        public IMemoryStream() : base() { }

        // convenience method for writing Strings to the stream
        public void Write(string s)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] pv = encoding.GetBytes(s);
            Write(pv, 0, pv.GetLength(0));
        }

        // Implementation of the IStream interface
        public void Clone(out IStream ppstm)
        {
            ppstm = null;
        }

        public void Read(byte[] pv, int cb, System.IntPtr pcbRead)
        {
            long bytesRead = Read(pv, 0, cb);
            if (pcbRead != IntPtr.Zero) Marshal.WriteInt64(pcbRead, bytesRead);
        }

        public void Write(byte[] pv, int cb, System.IntPtr pcbWritten)
        {
            Write(pv, 0, cb);
            if (pcbWritten != IntPtr.Zero) Marshal.WriteInt64(pcbWritten, (Int64)cb);
        }

        public void Seek(long dlibMove, int dwOrigin, System.IntPtr plibNewPosition)
        {
            long pos = base.Seek(dlibMove, (SeekOrigin)dwOrigin);
            if (plibNewPosition != IntPtr.Zero) Marshal.WriteInt64(plibNewPosition, pos);
        }

        public void SetSize(long libNewSize) 
        {
            int dummy = 0;
        }

        public void CopyTo(IStream pstm, long cb, System.IntPtr pcbRead, System.IntPtr pcbWritten) 
        {
            int dummy = 0;
        }

        public void Commit(int grfCommitFlags) 
        {
            int dummy = 0;
        }

        public void LockRegion(long libOffset, long cb, int dwLockType) 
        {
            int dummy = 0;
        }

        public void Revert() 
        {
            int dummy = 0;
        }

        public void UnlockRegion(long libOffset, long cb, int dwLockType) 
        {
            int dummy = 0;
        }

        public void Stat(out System.Runtime.InteropServices.STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new System.Runtime.InteropServices.STATSTG();
            pstatstg.cbSize = this.Length;
            pstatstg.pwcsName = "test.jpg";
            pstatstg.clsid = new Guid();

            
        }
    }
*/
}
