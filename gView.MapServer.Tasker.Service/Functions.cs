using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.system;

namespace gView.MapServer.Tasker.Service
{
    class Functions
    {
        static public string OgcOnlineResource(MapServerConfig.ServerConfig serverConfig)
        {
            return serverConfig + "/ogc";
        }
    }
}
