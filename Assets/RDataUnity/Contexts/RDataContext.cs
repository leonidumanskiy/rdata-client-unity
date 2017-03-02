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
        private const int FieldTrackingDepth = 100; // Snouldn't be ever close to this, this means that class we are trying to track is referencing itself. Don't do it!

        public TContextData Data { get; set; }
        
        private List<TrackedField> _trackedFields = new List<TrackedField>();

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
            CheckFieldsForTracking(Data, typeof(TContextData));
        }

        private void CheckFieldsForTracking(object obj, Type type = null, string keyPrefix = null, int depth = 0)
        {
            if (depth > FieldTrackingDepth)
                throw new RData.Exceptions.RDataException("Failed to track variable " + obj.GetType().ToString() + ", recursion depth exceeded");

            if (type == null)
                type = obj.GetType();

            var fields = type.GetFields().Where(field => Attribute.IsDefined(field, typeof(TrackVarAttribute)));
            foreach (var fieldInfo in fields)
            {
                var key = string.IsNullOrEmpty(keyPrefix) ? fieldInfo.Name : keyPrefix + "." + fieldInfo.Name;
                var trackedFieldInfo = new TrackedField() { key = key, obj = obj, fieldInfo = fieldInfo, lastValue = fieldInfo.GetValue(obj) };
                _trackedFields.Add(trackedFieldInfo);

                var fieldType = fieldInfo.FieldType;
                if (Attribute.IsDefined(fieldType, typeof(TrackClassAttribute)))
                {
                    CheckFieldsForTracking(fieldInfo.GetValue(obj), fieldType, key, depth + 1);
                }
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