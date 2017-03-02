using System;
using System.Reflection;

namespace RData.Contexts
{
    public class TrackedField
    {
        public object obj;
        public FieldInfo fieldInfo;
        public string key;
        public object lastValue;
        
        public object Value
        {
            get { return fieldInfo.GetValue(obj); }
        }

        public bool CheckFieldUpdated()
        {
            // Check if field is updated, if so, set the last value
            if(!lastValue.Equals(Value))
            {
                lastValue = Value;
                return true;
            } else
            {
                return false;
            }
        }
    }
}
