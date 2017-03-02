using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;
using LitJson;

namespace RData.Requests.Contexts
{
    public class SetContextDataRequest<TContextData> : JsonRpcRequest<SetContextDataRequest<TContextData>.Parameters, BooleanResponse>
        where TContextData : class, new()
    {
        [JsonAlias("method")]
        public override string Method
        {
            get { return "setContextData"; }
        }

        [JsonIgnore]
        public override bool IsBulked
        {
            get { return true; }
        }

        public class Parameters
        {
            [LitJson.JsonAlias("id")]
            public string Id { get; set; }
            
            [LitJson.JsonAlias("data")]
            public TContextData Data { get; set; }

            [LitJson.JsonAlias("timeSet")]
            public long TimeSet { get; set; }
        }

        public SetContextDataRequest() : base() { }

        public SetContextDataRequest(RDataContext<TContextData> context, TContextData data, long? timeSet = null) : this()
        {
            Params = new Parameters()
            {
                Id = context.Id,
                Data = data,
                TimeSet = timeSet.HasValue ? timeSet.Value : Tools.Time.UnixTime
            };
        }
    }
}