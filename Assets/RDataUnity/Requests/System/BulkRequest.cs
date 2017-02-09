using System;
using System.Collections.Generic;
using RData.JsonRpc;
using RData.Responses;
using RData.Events;

namespace RData.Requests.System
{
    public class BulkRequest : JsonRpcRequest<BulkRequest.Parameters, BooleanResponse>
    {
        public override string Method
        {
            get { return "bulkRequest"; }
        }

        public override bool IsBulked
        {
            get { return false; }
        }

        public class Parameters
        {
            [LitJson.JsonAlias("requests")]
            public List<JsonRpcBaseRequest> Requests { get; set; }
        }


        public int Length
        {
            get { return Params.Requests.Count; }
        }
        
        public BulkRequest() : base()
        {
            Params = new Parameters()
            {
                Requests = new List<JsonRpcBaseRequest>()
            };
        }

        public void AddRequest(JsonRpcBaseRequest request)
        {
            Params.Requests.Add(request);
        }
    }
}