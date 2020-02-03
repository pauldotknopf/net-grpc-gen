using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NetGrpcGen.Infra;

namespace NetGrpcGen.Adapters
{
    public class ObjectServiceAdapter<TObject,
        TGetPropRequest,
        TGetPropResponse,
        TCreateResponse,
        TStopRequest,
        TStopResponse> : ServiceBinderBase
        where TGetPropRequest : class, IObjectMessage, new()
        where TGetPropResponse : class, IMessage, new()
        where TCreateResponse : IObjectMessage, new()
        where TStopRequest : IMessage, new()
        where TStopResponse : IMessage, new()
    {
        private readonly ObjectAdapter<TObject, TGetPropRequest, TGetPropResponse> _objectAdapter;
        private readonly ConcurrentDictionary<ulong, TObject> _objects = new ConcurrentDictionary<ulong, TObject>();

        public ObjectServiceAdapter(ObjectAdapter<TObject, TGetPropRequest, TGetPropResponse> objectAdapter)
        {
            _objectAdapter = objectAdapter;
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

        private Task<TGetPropResponse> GetProperty(TGetPropRequest request)
        {
            if (!_objects.TryGetValue(request.ObjectId, out TObject o))
            {
                throw new Exception("Invalid object id.");
            }

            return Task.FromResult(_objectAdapter.GetProperty(o, request));
        }

        public class CustomServiceBinder : ServiceBinderBase
        {
            private readonly ServerServiceDefinition.Builder _builder;
            private readonly ObjectServiceAdapter<TObject, TGetPropRequest, TGetPropResponse, TCreateResponse, TStopRequest, TStopResponse> _serviceAdapter;

            public CustomServiceBinder(ServerServiceDefinition.Builder builder,
                ObjectServiceAdapter<TObject, TGetPropRequest, TGetPropResponse, TCreateResponse, TStopRequest, TStopResponse> serviceAdapter)
            {
                _builder = builder;
                _serviceAdapter = serviceAdapter;
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
                    case "GetProperty":
                        _builder.AddMethod(method, new UnaryServerMethod<TRequest, TResponse>(async (request, context) =>
                        {
                            var response = await _serviceAdapter.GetProperty(request as TGetPropRequest);
                            return response as TResponse;
                        }));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }
        
        public ServerServiceDefinition Create(Action<ServiceBinderBase> configure)
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            var binder = new CustomServiceBinder(builder, this);
            configure(binder);
            return builder.Build();
        }
    }
}