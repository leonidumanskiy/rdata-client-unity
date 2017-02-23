using System.Collections.Generic;

namespace RData.Contexts
{
    /// <summary>
    /// This class provides high-level data structure
    /// for the Context
    /// </summary>
    /// <typeparam name="TContextData">Type of context data</typeparam>
    public class RDataContext<TContextData> : RDataBaseContext
        where TContextData : class, new()
    {
        public TContextData Data { get; set; }

        public RDataContext(string id, string name, string parentContextId, TContextData data, RDataContextStatus status, System.DateTime timeStarted, System.DateTime timeEnded)
        {
            Id = id;
            Name = name;
            ParentContextId = parentContextId;
            Data = data;

            Status = status;
            TimeStarted = timeStarted;
            TimeEnded = timeEnded;

            Children = new List<RDataBaseContext>();
        }

        public RDataContext(TContextData data, RDataBaseContext parentContext = null) :
            this(System.Guid.NewGuid().ToString(), typeof(TContextData).Name, (parentContext != null ? parentContext.Id : null), data, RDataContextStatus.Started, System.DateTime.UtcNow, default(System.DateTime))
        {
        }

        public RDataContext() : 
            this(new TContextData())
        {
        }

        public override void End()
        {
            // End children contexts
            foreach (var childContext in Children)
                childContext.End();

            Status = RDataContextStatus.Ended;
            TimeEnded = System.DateTime.UtcNow;
        }

        public override void Restore()
        {
            Status = RDataContextStatus.Started;
            TimeEnded = default(System.DateTime);
        }

        public override void AddChild(RDataBaseContext context)
        {
            Children.Add(context);
            context.ParentContextId = Id;
        }
    }
}