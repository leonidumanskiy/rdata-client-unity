using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.LitJson;

namespace RData.Requests.Events
{
    public class LogEventRequest<TEventData> : JsonRpcRequest<LogEventRequest<TEventData>.Parameters, BooleanResponse>
    {
        [JsonAlias("method")]
        public override string Method
        {
            get { return "logEvent"; }
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

            [RData.LitJson.JsonAlias("contextId")]
            public string ContextId { get; set; }

            [RData.LitJson.JsonAlias("time")]
            public long Time { get; set; }

            [RData.LitJson.JsonAlias("data")]
            public TEventData Data { get; set; }
        }
        
        public LogEventRequest() : base() { }

        public LogEventRequest(RDataEvent<TEventData> eventData) : base()
        {
            Params = new Parameters()
            {
                Id = eventData.Id,
                Name = eventData.Name,
                ContextId = eventData.ContextId,
                Time = Tools.Time.DateTimeToUnixTimeMilliseconds(eventData.Time),
                Data = eventData.Data
            };
        }
    }
}