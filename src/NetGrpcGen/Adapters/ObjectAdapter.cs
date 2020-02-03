using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetGrpcGen.Infra;

namespace NetGrpcGen.Adapters
{
    public abstract class ObjectAdapter<TObject,
        TGetPropResponse,
        TSetPropRequest,
        TPropChanged,
        TPropertyEnum>
    {
        private readonly Dictionary<Type, object> _methodHandlers = new Dictionary<Type, object>();
        
        public abstract TObject Create();

        public abstract TPropertyEnum ParsePropertyEnum(string propertyName);

        public abstract void PackValue(TObject instance, TPropChanged dest);

        public abstract void PackValue(TObject instance, TGetPropResponse dest);
        
        public abstract void UnpackValue(TObject instance, TSetPropRequest source);

        public Task<TResponse> InvokeMethod<TRequest, TResponse>(TObject instance, TRequest request)
        {
            var type = request.GetType();
            var handler = (Func<TRequest, TObject, Task<TResponse>>) _methodHandlers[type];
            return handler(request, instance);
        }

        protected void RegisterMethod<TRequest, TResponse>(Func<TRequest, TObject, Task<TResponse>> function)
        {
            _methodHandlers.Add(typeof(TRequest), function);
        }
    }
}