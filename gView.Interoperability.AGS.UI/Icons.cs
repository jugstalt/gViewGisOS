using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;

namespace gView.Interoperability.AGS.UI
{
    internal class AGSIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("07DAB2C3-6589-4587-B71E-2F5CC39753B3"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Interoperability.AGS.UI.Properties.Resources.computer; }
        }

        #endregion
    }
    internal class AGSNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("EBED43E0-8366-4ed0-AEF4-F084D61A4DE8"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Interoperability.AGS.UI.Properties.Resources.i_connection_server; }
        }

        #endregion
    }
    internal class AGSConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("D81AA35A-678A-4ee4-A992-086CBBD9DFA9"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Interoperability.AGS.UI.Properties.Resources.computer_go; }
        }

        #endregion
    }
    internal class AGSServiceIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("63CD10C4-845B-435a-B072-84D6DBBB970A"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Interoperability.AGS.UI.Properties.Resources.i_connection; }
        }

        #endregion
    }
}
