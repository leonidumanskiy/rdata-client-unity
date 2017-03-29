using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;
using System;
using RData.LitJson;

namespace RData.Requests.Contexts
{
    public class RestoreContextRequest : JsonRpcRequest<RestoreContextRequest.Parameters, BooleanResponse>
    {
        [JsonAlias("method")]
        public override string Method
        {
            get { return "restoreContext"; }
        }

        [JsonIgnore]
        public override bool IsBulked
        {
            get { return true; }
        }

        public class Parameters
        {
            [JsonAlias("id")]
            public string Id { get; set; }

            [JsonAlias("timeRestored")]
            public long TimeRestored { get; set; }
        }

        public RestoreContextRequest() : base() { }

        public RestoreContextRequest(RDataBaseContext context) : base()
        {
            Params = new Parameters()
            {
                Id = context.Id,
                TimeRestored = Tools.Time.DateTimeToUnixTimeMilliseconds(DateTime.UtcNow)
            };
        }

    }
}