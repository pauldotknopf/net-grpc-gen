using System;
using System.Collections.Generic;

namespace NetGrpcGen.Adapters
{
    public abstract class ObjectAdapter<TObject>
    {
        private readonly Dictionary<string, Type> _propChangedEvents = new Dictionary<string, Type>();
        
        public abstract TObject Create();

        public Type GetPropChangedType(string propName)
        {
            return _propChangedEvents.ContainsKey(propName) ? _propChangedEvents[propName] : null;
        }

        protected void RegisterPropChangedType<T>(string propName)
        {
            _propChangedEvents[propName] = typeof(T);
        }
    }
}