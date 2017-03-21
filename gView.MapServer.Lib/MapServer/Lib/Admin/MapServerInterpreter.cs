using System;
using System.Collections.Generic;
using System.Text;
using gView.MapServer;

namespace gView.MapServer.Lib.Admin
{
    [gView.Framework.system.RegisterPlugIn("FDDF09E4-DE73-41af-B09C-DCB7CC94B29D")]
    class MapServerInterpreter : IServiceRequestInterpreter
    {
        private IMapServer _mapServer = null;

        #region IServiceRequestInterpreter Member

        public void OnCreate(IMapServer mapServer)
        {
            _mapServer = mapServer;
        }

        public void Request(IServiceRequestContext context)
        {
            if (context == null || context.ServiceRequest == null)
                return;

            if (_mapServer == null)
            {
                context.ServiceRequest.Response = "<FATALERROR>MapServer Object is not available!</FATALERROR>";
                return;
            }

            switch (context.ServiceRequest.Request)
            {
                case "options":
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<MapServer><Options>");
                    sb.Append("<OutputPath>" + _mapServer.OutputPath + "</OutputPath>");
                    sb.Append("<OutputUrl>" + _mapServer.OutputUrl + "</OutputUrl>");
                    sb.Append("<TileCachePath>" + _mapServer.TileCachePath + "</TileCachePath>");
                    sb.Append("</Options></MapServer>");
                    context.ServiceRequest.Response = sb.ToString();
                    break;
            }
        }

        public string IntentityName
        {
            get { return String.Empty; }
        }

        public InterpreterCapabilities Capabilities
        {
            get { return null; }
        }

        #endregion
    }
}
