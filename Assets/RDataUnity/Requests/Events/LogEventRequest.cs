using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.LitJson;

namespace RData.Requests.Events
{
    public class LogEventRequest<TEventData> : JsonRpcRequest<LogEventRequest<TEventData>.Parameters, BooleanResponse>
        where TEventData : class, new()
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

            [RData.LitJson.JsonAlias("eventDataVersion")]
            public int EventDataVersion { get; set; }

            [RData.LitJson.JsonAlias("data")]
            public TEventData Data { get; set; }
        }
        
        public LogEventRequest() : base() { }

        public LogEventRequest(RDataEvent<TEventData> evt) : base()
        {
            Params = new Parameters()
            {
                Id = evt.Id,
                Name = evt.Name,
                ContextId = evt.ContextId,
                Time = Tools.Time.DateTimeToUnixTimeMilliseconds(evt.Time),
                Data = evt.Data,
                EventDataVersion = evt.EventDataVersion
            };
        }
    }
}