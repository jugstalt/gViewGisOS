using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.Oracle.UI
{
    class OracleIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("9D064033-D037-4A9E-BD5D-6684383C1199");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.DataSources.Oracle.UI.Properties.Resources.cat6;
            }
        }

        #endregion
    }

    class OracleNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("1D199B75-5357-4301-B1A3-2A82BDABC6F2");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.DataSources.Oracle.UI.Properties.Resources.gps_point;
            }
        }

        #endregion
    }

    public class OraclePointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("4FD3AA63-A1B2-48EB-9C06-D9772DEB789C"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Oracle.UI.Properties.Resources.img_32; }
        }

        #endregion
    }

    public class OracleLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("D65C95B2-7EDE-4808-AFB2-9B7436E506FF"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Oracle.UI.Properties.Resources.img_33; }
        }

        #endregion
    }

    public class OraclePolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("307F6AAB-1E33-4B00-A7F7-3F9C0E5E3CEF"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Oracle.UI.Properties.Resources.img_34; }
        }

        #endregion
    }
}
