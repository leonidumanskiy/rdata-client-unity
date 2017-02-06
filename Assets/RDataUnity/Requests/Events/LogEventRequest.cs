using RData.JsonRpc;
using RData.Responses;
using RData.Events;

namespace RData.Requests.Events
{
    public class LogEventRequest<TEventData> : JsonRpcRequest<LogEventRequest<TEventData>.Parameters, BooleanResponse>
    {
        public override string Method
        {
            get { return "logEvent"; }
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
        
        public LogEventRequest(RDataEvent<TEventData> eventData)
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