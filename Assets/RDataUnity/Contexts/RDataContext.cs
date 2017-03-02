using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

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
        
        private List<TrackedField> _trackedFields;

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

            // Build the tracked fields dictionary
            var type = typeof(TContextData);
            _trackedFields = new List<TrackedField>();
            /*
            var props = type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(TrackVarAttribute)));
            foreach(var prop in props)
            {
                var trackVarInfo = new TrackVarInfo() { trackedObject = Data, trackedMemberInfo = prop, trackedValue = prop.GetValue(data, null) };
            }
            */

            // TODO: Check the type and go recursively if the type is a reference or list etc
            var fields = type.GetFields().Where(field => Attribute.IsDefined(field, typeof(TrackVarAttribute)));
            foreach(var fieldInfo in fields)
            {
                var trackedFieldInfo = new TrackedField() { key = fieldInfo.Name, obj = Data, fieldInfo = fieldInfo, lastValue = fieldInfo.GetValue(data) };
                _trackedFields.Add(trackedFieldInfo);
            }
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

        public override IEnumerable<KeyValuePair<string, object>> GetUpdatedFields()
        {            
            return _trackedFields
                .Where(field => field.CheckFieldUpdated())
                .Select(field => new KeyValuePair<string, object>(field.key, field.lastValue));
        }
    }
}