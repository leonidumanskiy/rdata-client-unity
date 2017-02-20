using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;
using System;

namespace RData.Requests.Contexts
{
    public class RestoreContextRequest : JsonRpcRequest<RestoreContextRequest.Parameters, BooleanResponse>
    {
        public override string Method
        {
            get { return "restoreContext"; }
        }

        public override bool IsBulked
        {
            get { return true; }
        }

        public class Parameters
        {
            [LitJson.JsonAlias("id")]
            public string Id { get; set; }

            [LitJson.JsonAlias("timeRestored")]
            public long TimeRestored { get; set; }
        }

        public RestoreContextRequest() : base() { }

        public RestoreContextRequest(RDataBaseContext context) : base()
        {
            Params = new Parameters()
            {
                Id = context.Id,
                TimeRestored = Tools.Time.DateTimeToUnixTime(DateTime.UtcNow)
            };
        }

    }
}