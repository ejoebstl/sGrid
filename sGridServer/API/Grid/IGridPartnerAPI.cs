using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace sGridServer.API.Grid
{
    [ServiceContract]
    public interface IGridPartnerAPI
    {
        [OperationContract]
        [WebGet(UriTemplate = "feeded/{projectId}/{userId}/{workunitId}/", ResponseFormat = WebMessageFormat.Xml)]
        void FeededWorkUnit(string projectId, string workunitId, string userId);

        [OperationContract]
        [WebGet(UriTemplate = "returned/{projectId}/{userId}/{workunitId}/", ResponseFormat = WebMessageFormat.Xml)]
        void ReceivedResult(string projectId, string workunitId, string userId);

        [OperationContract]
        [WebGet(UriTemplate = "validated/{projectId}/{userId}/{workunitId}/{success}/", ResponseFormat = WebMessageFormat.Xml)]
        void ValidatedResult(string projectId, string workunitId, string userId, string success);
    }
}
