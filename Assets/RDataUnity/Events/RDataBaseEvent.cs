using RData.LitJson;

namespace RData.Events
{
    public abstract class RDataBaseEvent
    {
        public string Id { get; set; }

        public string ContextId { get; set; }

        public string Name { get; set; }

        public System.DateTime Time { get; set; }
    }
}