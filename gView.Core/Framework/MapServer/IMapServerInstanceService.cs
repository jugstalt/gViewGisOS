using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace gView.Framework.MapServer
{
    [ServiceContract]
    public interface IMapServerInstanceService
    {
        [OperationContract]
        string ServiceRequest(string service, string request, string InterpreterGUID, string usr, string pwd);
        [OperationContract]
        string ServiceRequest2(string OnlineResource, string service, string request, string InterpreterGUID, string usr, string pwd);

        [OperationContract]
        bool AddMap(string mapName, string MapXML, string usr, string pwd);
        [OperationContract]
        bool RemoveMap(string mapName, string usr, string pwd);

        [OperationContract]
        string Ping();

        [OperationContract]
        string GetMetadata(string mapName, string usr, string pwd);
        [OperationContract]
        bool SetMetadata(string mapName, string metadata, string usr, string pwd);

        [OperationContract]
        bool ReloadMap(string mapName, string usr, string pwd);
    }
}
