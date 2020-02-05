using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NetGrpcGen.Infra;
using NetGrpcGen.Model;

namespace NetGrpcGen.Adapters
{
    public class ObjectServiceAdapter<TObject,
        TCreateResponse,
        TStopRequest,
        TStopResponse>
        where TCreateResponse : IObjectMessage, new()
        where TStopRequest : IMessage, new()
        where TStopResponse : IMessage, new()
    {
        private readonly ObjectAdapter<TObject> _objectAdapter;
        private readonly GrpcObject _grpcObject;
        private readonly ConcurrentDictionary<ulong, TObject> _objects = new ConcurrentDictionary<ulong, TObject>();

        public ObjectServiceAdapter(
            ObjectAdapter<TObject> objectAdapter,
            GrpcObject grpcObject)
        {
            _objectAdapter = objectAdapter;
            _grpcObject = grpcObject;
        }

        private async Task Create(
            IAsyncStreamReader<Any> requestStream,
            IServerStreamWriter<Any> responseStream,
            ServerCallContext context)
        {
            var o = _objectAdapter.Create();
            var tagId = o.GetOrCreateTag();
            _objects.TryAdd(tagId, o);

            try
            {
                var response = new TCreateResponse {ObjectId = tagId};
                await responseStream.WriteAsync(Any.Pack(response));

                // Wait for a stop request...
                await requestStream.MoveNext();
                requestStream.Current.Unpack<TStopRequest>();

                // Send the stop response.
                await responseStream.WriteAsync(Any.Pack(new TStopResponse()));
            }
            finally
            {
                ObjectTagger.Default.FreeId(tagId);
                _objects.TryRemove(tagId, out o);
            }
        }
        
        private async Task<TResponse> InvokeMethod<TRequest, TResponse>(GrpcMethod method, TRequest request)
        {
            var objectMessage = request as IObjectMessage;
            if (objectMessage == null)
            {
                throw new Exception("Request type doesn't implement IObjectMessage.");
            }
            
            if (!_objects.TryGetValue(objectMessage.ObjectId, out TObject o))
            {
                throw new Exception("Invalid object id.");
            }

            var response = method.Method.Invoke(o, new object[] { request });

            if (response is Task task)
            {
                await task;
                var result = (object)((dynamic)task).Result;
                return (TResponse)result;
            }
                
            return (TResponse)response;
        }
       
        public class CustomServiceBinder : ServiceBinderBase
        {
            private readonly ServerServiceDefinition.Builder _builder;
            private readonly ObjectServiceAdapter<TObject,
                TCreateResponse,
                TStopRequest,
                TStopResponse> _serviceAdapter;
            private readonly GrpcObject _grpcObject;

            public CustomServiceBinder(ServerServiceDefinition.Builder builder,
                ObjectServiceAdapter<TObject,
                    TCreateResponse,
                    TStopRequest,
                    TStopResponse> serviceAdapter,
                GrpcObject grpcObject)
            {
                _builder = builder;
                _serviceAdapter = serviceAdapter;
                _grpcObject = grpcObject;
            }

            public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, DuplexStreamingServerMethod<TRequest, TResponse> handler)
            {
                switch (method.Name)
                {
                    case "Create":
                        _builder.AddMethod(method, new DuplexStreamingServerMethod<TRequest, TResponse>(
                            (stream, responseStream, context) => _serviceAdapter.Create(stream as IAsyncStreamReader<Any>,
                                responseStream as IServerStreamWriter<Any>, context)));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ClientStreamingServerMethod<TRequest, TResponse> handler)
            {
                throw new NotSupportedException();
            }

            public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServerStreamingServerMethod<TRequest, TResponse> handler)
            {
                throw new NotSupportedException();
            }

            public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
            {
                switch (method.Name)
                {
                    default:
                        bool found = false;
                        foreach (var invokeMethod in _grpcObject.Methods)
                        {
                            if (invokeMethod.Name == method.Name)
                            {
                                found = true;
                                _builder.AddMethod(method, async (request, context) => await _serviceAdapter.InvokeMethod<TRequest, TResponse>(invokeMethod, request));
                                break;
                            }
                        }
                        if (!found)
                        {
                            throw new NotSupportedException();
                        }
                        break;
                }
            }
        }
        
        public ServerServiceDefinition Create(Action<ServiceBinderBase> configure)
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            var binder = new CustomServiceBinder(builder, this, _grpcObject);
            configure(binder);
            return builder.Build();
        }
    }
}