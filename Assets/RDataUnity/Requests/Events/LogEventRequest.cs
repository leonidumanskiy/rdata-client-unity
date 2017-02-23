using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using LitJson;

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
            [LitJson.JsonAlias("id")]
            public string Id { get; set; }

            [LitJson.JsonAlias("name")]
            public string Name { get; set; }

            [LitJson.JsonAlias("contextId")]
            public string ContextId { get; set; }

            [LitJson.JsonAlias("time")]
            public long Time { get; set; }

            [LitJson.JsonAlias("data")]
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
                Time = Tools.Time.DateTimeToUnixTime(eventData.Time),
                Data = eventData.Data
            };
        }
    }
}