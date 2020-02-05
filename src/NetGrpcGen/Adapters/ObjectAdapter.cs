using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetGrpcGen.Infra;

namespace NetGrpcGen.Adapters
{
    public abstract class ObjectAdapter<TObject>
    {
        private readonly Dictionary<Type, object> _methodHandlers = new Dictionary<Type, object>();
        
        public abstract TObject Create();

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