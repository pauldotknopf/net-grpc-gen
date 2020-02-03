using System;
using System.Collections.Concurrent;
using System.ComponentModel;
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
        TSetPropRequest,
        TSetPropResponse,
        TPropChanged,
        TCreateResponse,
        TStopRequest,
        TStopResponse,
        TPropertyEnum>
        where TGetPropRequest : class, IObjectMessage, IPropertyMessage<TPropertyEnum>, new()
        where TGetPropResponse : class, IObjectMessage, IPropertyMessage<TPropertyEnum>, new()
        where TSetPropRequest : class, IObjectMessage, IPropertyMessage<TPropertyEnum>, new()
        where TSetPropResponse : class, IObjectMessage, IPropertyMessage<TPropertyEnum>, new()
        where TPropChanged : class, IObjectMessage, IPropertyMessage<TPropertyEnum>, new()
        where TCreateResponse : IObjectMessage, new()
        where TStopRequest : IMessage, new()
        where TStopResponse : IMessage, new()
    {
        private readonly ObjectAdapter<TObject, TGetPropResponse, TSetPropRequest, TPropChanged, TPropertyEnum> _objectAdapter;
        private readonly ConcurrentDictionary<ulong, TObject> _objects = new ConcurrentDictionary<ulong, TObject>();

        public ObjectServiceAdapter(ObjectAdapter<TObject, TGetPropResponse, TSetPropRequest, TPropChanged, TPropertyEnum> objectAdapter)
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

                var handler = new PropertyChangedEventHandler((sender, args) =>
                {
                    var prop = _objectAdapter.ParsePropertyEnum(args.PropertyName);
                    var message = new TPropChanged();
                    message.ObjectId = tagId;
                    message.Prop = prop;
                    _objectAdapter.PackValue(o, message);
                    // TODO: Queue request on the outer context.
                    responseStream.WriteAsync(Any.Pack(message));
                });
                var propChanged = o as INotifyPropertyChanged;
                if(propChanged != null)
                {
                    propChanged.PropertyChanged += handler;
                }
                
                // Wait for a stop request...
                await requestStream.MoveNext();
                requestStream.Current.Unpack<TStopRequest>();

                if (propChanged != null)
                {
                    propChanged.PropertyChanged -= handler;
                }
                
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

            var response = new TGetPropResponse
            {
                Prop = request.Prop,
                ObjectId = request.ObjectId
            };
            _objectAdapter.PackValue(o, response);

            return Task.FromResult(response);
        }
        
        private Task<TSetPropResponse> SetProperty(TSetPropRequest request)
        {
            if (!_objects.TryGetValue(request.ObjectId, out TObject o))
            {
                throw new Exception("Invalid object id.");
            }
            
            _objectAdapter.UnpackValue(o, request);

            return Task.FromResult(new TSetPropResponse
            {
                Prop = request.Prop,
                ObjectId = request.ObjectId
            });
        }

        public class CustomServiceBinder : ServiceBinderBase
        {
            private readonly ServerServiceDefinition.Builder _builder;
            private readonly ObjectServiceAdapter<TObject,
                TGetPropRequest,
                TGetPropResponse,
                TSetPropRequest,
                TSetPropResponse,
                TPropChanged,
                TCreateResponse,
                TStopRequest,
                TStopResponse,
                TPropertyEnum> _serviceAdapter;

            public CustomServiceBinder(ServerServiceDefinition.Builder builder,
                ObjectServiceAdapter<TObject,
                    TGetPropRequest,
                    TGetPropResponse,
                    TSetPropRequest,
                    TSetPropResponse,
                    TPropChanged,
                    TCreateResponse,
                    TStopRequest,
                    TStopResponse,
                    TPropertyEnum> serviceAdapter)
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
                    case "SetProperty":
                        _builder.AddMethod(method, new UnaryServerMethod<TRequest, TResponse>(async (request, context) =>
                        {
                            var response = await _serviceAdapter.SetProperty(request as TSetPropRequest);
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