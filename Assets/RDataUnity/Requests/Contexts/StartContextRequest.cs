using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;
using LitJson;

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
            [LitJson.JsonAlias("id")]
            public string Id { get; set; }

            [LitJson.JsonAlias("name")]
            public string Name { get; set; }
            
            [LitJson.JsonAlias("parentContextId")]
            public string ParentContextId { get; set; }

            [LitJson.JsonAlias("data")]
            public TContextData Data { get; set; }

            [LitJson.JsonAlias("timeStarted")]
            public long TimeStarted { get; set; }
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
                TimeStarted = Tools.Time.DateTimeToUnixTime(context.TimeStarted)
            };
        }

    }
}