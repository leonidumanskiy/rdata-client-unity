using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;
using RData.LitJson;

namespace RData.Requests.Contexts
{
    public class StartContextRequest<TContextData> : JsonRpcRequest<StartContextRequest<TContextData>.Parameters, BooleanResponse>
        where TContextData : class, new()
    {
        [JsonAlias("method")]
        public override string Method
        {
            get { return "startContext"; }
        }

        [JsonIgnore]
        public override bool IsBulked
        {
            get { return true; }
        }

        public class Parameters
        {
            [RData.LitJson.JsonAlias("id")]
            public string Id { get; set; }

            [RData.LitJson.JsonAlias("name")]
            public string Name { get; set; }
            
            [RData.LitJson.JsonAlias("parentContextId")]
            public string ParentContextId { get; set; }

            [RData.LitJson.JsonAlias("timeStarted")]
            public long TimeStarted { get; set; }

            [RData.LitJson.JsonAlias("contextDataVersion")]
            public int ContextDataVersion { get; set; }

            [RData.LitJson.JsonAlias("data")]
            public TContextData Data { get; set; }

        }

        public StartContextRequest() : base() { }

        public StartContextRequest(RDataContext<TContextData> context) : base()
        {
            Params = new Parameters()
            {
                Id = context.Id,
                Name = context.Name,
                ParentContextId = context.ParentContextId,
                Data = context.Data,
                TimeStarted = Tools.Time.DateTimeToUnixTimeMilliseconds(context.TimeStarted),
                ContextDataVersion = context.ContextDataVersion
            };
        }

    }
}