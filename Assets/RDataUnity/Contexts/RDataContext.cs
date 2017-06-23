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
    public abstract class RDataContext<TContextData> : RDataBaseContext
        where TContextData : class, new()
    {
        private const int FieldTrackingDepth = 100; // Snouldn't be ever close to this, this means that class we are trying to track is referencing itself. Don't do it!
        
        public TContextData Data { get; private set; } // Private set to prevent people from setting it directly (use constructor chaining to create a context with pre-defined data)
        
        private List<TrackedField> _trackedFields = new List<TrackedField>();

        public RDataContext(string id, RDataBaseContext parentContext, TContextData data, RDataContextStatus status, System.DateTime timeStarted, System.DateTime timeEnded)
        {
            Id = id;
            Name = GetType().Name;
            Parent = parentContext;
            Data = data;

            Status = status;
            TimeStarted = timeStarted;
            TimeEnded = timeEnded;

            Children = new List<RDataBaseContext>();

            // Build the tracked fields dictionary
            CheckFieldsForTracking(Data, typeof(TContextData));
        }

        public RDataContext(TContextData data, RDataBaseContext parentContext = null) :
            this(System.Guid.NewGuid().ToString(), parentContext, data, RDataContextStatus.Started, System.DateTime.UtcNow, default(System.DateTime))
        {
        }

        public RDataContext() : 
            this(new TContextData())
        {
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

        public override void End()
        {
            // End children contexts
            for (int i = Children.Count-1; i >= 0; i--)
            {
                var childContext = Children[i];
                childContext.End();
            }
            
            Status = RDataContextStatus.Ended;
            TimeEnded = System.DateTime.UtcNow;

            if(Parent != null)
                Parent.RemoveChild(this);
        }

        public override void Restore()
        {
            Status = RDataContextStatus.Started;
            TimeEnded = default(System.DateTime);
        }

        public override void AddChild(RDataBaseContext context)
        {
            Children.Add(context);
            context.Parent = this;
        }
        
        public override void RemoveChild(RDataBaseContext context)
        {
            Children.Remove(context);
        }

        public override IEnumerable<KeyValuePair<string, object>> GetUpdatedFields()
        {            
            return _trackedFields
                .Where(field => field.CheckFieldUpdated())
                .Select(field => new KeyValuePair<string, object>(field.key, field.lastValue));
        }
    }
}