using RData.Contexts;
namespace RData.Events
{
    public abstract class RDataEvent<TEventData> : RDataBaseEvent
        where TEventData : class, new()
    {
        public TEventData Data { get; set; }
        
        public RDataEvent(string id, System.DateTime time, TEventData data, string contextId = null)
        {
            Id = id;
            ContextId = contextId;
            Name = GetType().Name;
            Time = time;
            Data = data;
        }

        public RDataEvent(TEventData data, RDataBaseContext context = null) 
            : this(System.Guid.NewGuid().ToString(), System.DateTime.UtcNow, data, (context != null ? context.Id : null))
        {
        }

        public RDataEvent()
            : this(new TEventData())
        {
        }
    }
}