using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace gView.MapServer.Tasker.Service
{
    [ServiceContract]
    public interface ITaskerService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/Index")]
        Stream Index();

        [OperationContract]
        [WebGet(UriTemplate = "/Catalog")]
        Stream Catalog();

        [OperationContract]
        [WebGet(UriTemplate = "/RecycleAll")]
        Stream RecycleAll();

        [OperationContract]
        [WebGet(UriTemplate = "/ServiceCapabilities")]
        Stream ServiceCapabilities();

        [OperationContract]
        [WebGet(UriTemplate = "/ExploreMap/{name}")]
        Stream ExploreMap(string name);

        [OperationContract]
        [WebGet(UriTemplate = "/file/{name}")]
        Stream LoadHtmlFile(string name);

        [OperationContract]
        [WebInvoke(UriTemplate = "/AddMap/{name}", Method = "POST")]
        Stream AddMap(string name, Stream data);

        [OperationContract]
        [WebGet(UriTemplate="/RemoveMap/{name}")]
        Stream RemoveMap(string name);

        [OperationContract]
        [WebGet(UriTemplate="/GetMetadata/{name}")]
        Stream GetMetadata(string name);

        [OperationContract]
        [WebInvoke(UriTemplate="SetMetadata/{name}",Method="POST")]
        Stream SetMetadata(string name,Stream data);

        [OperationContract]
        [WebGet(UriTemplate = "/MapRequest/{interpreter}/{name}")]
        Stream MapRequestGet(string interpreter, string name);

        [OperationContract] 
        [WebInvoke(UriTemplate = "/MapRequest/{interpreter}/{name}", Method = "POST")]
        Stream MapRequestPost(string interpreter,string name, Stream data);

        [OperationContract]
        [WebGet(UriTemplate = "/servlet/com.esri.esrimap.Esrimap")]
        Stream AxlGet();

        [OperationContract]
        [WebInvoke(UriTemplate = "/servlet/com.esri.esrimap.Esrimap", Method = "POST")]
        Stream AxlPost(Stream data);

        [OperationContract]
        [WebGet(UriTemplate = "/TileWmts/{name}/{cachetype}/{origin}/{epsg}/{style}/{level}/{row}/{col}")]
        Stream TileWmts(string name, string cachetype, string origin, string epsg, string style, string level, string row, string col);
    }
}
