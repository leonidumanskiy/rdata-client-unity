using RData.Contexts;
namespace RData.Events
{
    public class RDataEvent<TEventData> : RDataBaseEvent
    {
        public TEventData Data { get; set; }
        
        public RDataEvent(string id, string name, System.DateTime time, TEventData data, string contextId = null)
        {
            Id = id;
            ContextId = contextId;
            Name = name;
            Time = time;
            Data = data;
        }

        public RDataEvent(TEventData data, RDataBaseContext context = null) 
            : this(System.Guid.NewGuid().ToString(), typeof(TEventData).Name, System.DateTime.UtcNow, data, (context != null ? context.Id : null))
        {
        }
    }
}