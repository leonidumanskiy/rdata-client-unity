using System.Collections.Generic;

namespace RData.Contexts
{
    public abstract class RDataBaseContext
    {
        public string Id { get; set; }

        public string Name { get; set; }
        
        public string ParentContextId { get; set; }

        public List<RDataBaseContext> Children { get; set; }

        public RDataContextStatus Status { get; set; }

        public System.DateTime TimeStarted { get; set; }

        public System.DateTime TimeEnded { get; set; }


        public virtual void End() { }

        public virtual void Restore() { }

        public virtual void AddChild(RDataBaseContext context) { }

        public virtual IEnumerable<KeyValuePair<string, object>> GetUpdatedFields() { return null; }
    }
}