using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NetGrpcGen.Infra;
using NetGrpcGen.Model;
using Type = System.Type;

namespace NetGrpcGen.Adapters
{
    public class ObjectServiceAdapter<TObject,
        TCreateResponse,
        TStopRequest,
        TStopResponse>
        where TCreateResponse : IMessage, new()
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

        private async Task ListenEvents(
            object request,
            IServerStreamWriter<Any> responseStream,
            ServerCallContext context)
        {
            var objectId = GetObjectId(request);

            if (!_objects.TryGetValue(objectId, out TObject o))
            {
                throw new Exception("Invalid object id.");
            }

            var eventHandlers = new List<Tuple<GrpcEvent, Delegate>>();
            foreach (var even in _grpcObject.Events)
            {
                var del = Create(even.Event, val =>
                {
                    var eventType = _objectAdapter.GetEventType(even.Name);
                    if (eventType == null)
                    {
                        return;
                    }

                    var eventMessage = Activator.CreateInstance(eventType) as IMessage;
                    SetObjectId(eventMessage, objectId);
                    if (even.DataType != null)
                    {
                        SetValue(eventMessage, val);
                    }

                    responseStream.WriteAsync(Any.Pack(eventMessage));
                });
                even.Event.AddEventHandler(o, del);
                eventHandlers.Add(new Tuple<GrpcEvent, Delegate>(even, del));
            }

            var notifyHandler = o as INotifyPropertyChanged;
            var handler = new PropertyChangedEventHandler((sender, args) =>
            {
                var grpcProperty = _grpcObject.Properties.SingleOrDefault(x => x.Name == args.PropertyName);
                if (grpcProperty == null)
                {
                    return;
                }

                var eventType = _objectAdapter.GetPropChangedType(args.PropertyName);
                if (eventType == null)
                {
                    throw new Exception($"Couldn't get event type for property {args.PropertyName}.");
                }

                var propChangedResponse = Activator.CreateInstance(eventType);
                SetValue(propChangedResponse, grpcProperty.Property.GetValue(o));
                SetObjectId(propChangedResponse, objectId);
                responseStream.WriteAsync(Any.Pack(propChangedResponse as IMessage));
            });
            if (notifyHandler != null)
            {
                notifyHandler.PropertyChanged += handler;
            }
            
            try
            {
                // Wait for the client to disconnect.
                var tcs = new TaskCompletionSource<bool>();
                context.CancellationToken.Register(s => ((TaskCompletionSource<bool>) s).SetResult(true), tcs);
                if (!context.CancellationToken.IsCancellationRequested)
                {
                    await tcs.Task;
                }
            }
            finally
            {
                // Remove all the event handlers.
                foreach (var even in eventHandlers)
                {
                    even.Item1.Event.RemoveEventHandler(o, even.Item2);
                }
                if (notifyHandler != null)
                {
                    notifyHandler.PropertyChanged -= handler;
                }
            }
        }

        private async Task Create(
            IAsyncStreamReader<Any> requestStream,
            IServerStreamWriter<Any> responseStream)
        {
            var o = _objectAdapter.Create();
            var tagId = o.GetOrCreateTag();
            _objects.TryAdd(tagId, o);

            try
            {
                var response = new TCreateResponse();
                SetObjectId(response, tagId);
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
        
        private Delegate Create(EventInfo evt, Action<object> d)
        {
            var handlerType = evt.EventHandlerType;
            var eventParams = handlerType.GetGenericArguments();

            //lambda: (object x0, ExampleEventArgs x1) => d(x1.IntArg)
            var parameters = eventParams.Select(p=>Expression.Parameter(p,"x")).ToArray();
            Expression body = null;
            if (parameters.Length == 1)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                body = Expression.Call(Expression.Constant(d), d.GetType().GetMethod("Invoke"), parameters[0]);
            }
            else
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                body = Expression.Call(Expression.Constant(d), d.GetType().GetMethod("Invoke"), Expression.Constant(null));
            }
            var lambda = Expression.Lambda(body,parameters);

            var compileDel = lambda.Compile();

            var m = compileDel.GetType().GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);

            return Delegate.CreateDelegate(handlerType, compileDel, m);
        }
        
        private async Task<TResponse> InvokeMethod<TRequest, TResponse>(GrpcMethod method, TRequest request)
        {
            var objectId = GetObjectId(request);
            
            if (!_objects.TryGetValue(objectId, out TObject o))
            {
                throw new Exception("Invalid object id.");
            }

            var value = GetValue(request);
            
            var response = method.Method.Invoke(o, new object[] { value });

            if (response is Task task)
            {
                await task;
                var result = (object)((dynamic)task).Result;
                var messageResponse = Activator.CreateInstance(typeof(TResponse));
                SetValue(messageResponse, result);
                return (TResponse)messageResponse;
            }
            else
            {
                var messageResponse = Activator.CreateInstance(typeof(TResponse));
                if (method.ResponseType != null)
                {
                    SetValue(messageResponse, response);
                }
                return (TResponse) messageResponse;
            }
        }

        private TResponse GetProperty<TRequest, TResponse>(GrpcProperty property, TRequest request)
        {
            var objectId = GetObjectId(request);
            
            if (!_objects.TryGetValue(objectId, out TObject o))
            {
                throw new Exception("Invalid object id.");
            }

            var response = Activator.CreateInstance(typeof(TResponse));

            var value = property.Property.GetValue(o);
            SetValue(response, value);
            
            return (TResponse)response;
        }
        
        private TResponse SetProperty<TRequest, TResponse>(GrpcProperty property, TRequest request)
        {
            var objectId = GetObjectId(request);
            
            if (!_objects.TryGetValue(objectId, out TObject o))
            {
                throw new Exception("Invalid object id.");
            }

            var valueToSet = GetValue(request);

            property.Property.SetValue(o, valueToSet);

            return (TResponse)Activator.CreateInstance(typeof(TResponse));
        }

        private ulong GetObjectId(object instance)
        {
            var prop = instance.GetType().GetProperty("ObjectId", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                throw new Exception($"The type {instance.GetType().Name} doesn't have a property \"ObjectId\".");
            }
            return (ulong)prop.GetValue(instance);
        }
        
        private void SetObjectId(object instance, ulong value)
        {
            var prop = instance.GetType().GetProperty("ObjectId", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                throw new Exception($"The type {instance.GetType().Name} doesn't have a property \"ObjectId\".");
            }
            prop.SetValue(instance, value);
        }

        private void SetValue(object instance, object value)
        {
            var prop = instance.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                throw new Exception($"The type {instance.GetType().Name} doesn't have a property \"Value\".");
            }
            prop.SetValue(instance, value);
        }
        
        private object GetValue(object instance)
        {
            var prop = instance.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                throw new Exception($"The type {instance.GetType().Name} doesn't have a property \"Value\".");
            }
            return prop.GetValue(instance);
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
                                responseStream as IServerStreamWriter<Any>)));
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
                switch (method.Name)
                {
                    case "ListenEvents":
                        _builder.AddMethod(method, new ServerStreamingServerMethod<TRequest, TResponse>(
                            (request, responseStream, context) => _serviceAdapter.ListenEvents(request,
                                responseStream as IServerStreamWriter<Any>, context)));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
            {
                switch (method.Name)
                {
                    default:
                        foreach (var invokeMethod in _grpcObject.Methods)
                        {
                            if (invokeMethod.Name == method.Name)
                            {
                                _builder.AddMethod(method, async (request, context) => await _serviceAdapter.InvokeMethod<TRequest, TResponse>(invokeMethod, request));
                                return;
                            }
                        }

                        foreach (var property in _grpcObject.Properties)
                        {
                            if (method.Name == $"GetProperty{property.Name}")
                            {
                                _builder.AddMethod(method, (request, context) =>
                                {
                                    var response = _serviceAdapter.GetProperty<TRequest, TResponse>(property, request);
                                    return Task.FromResult(response);
                                });
                                return;
                            }

                            if (method.Name == $"SetProperty{property.Name}")
                            {
                                _builder.AddMethod(method, (request, context) =>
                                {
                                    var response = _serviceAdapter.SetProperty<TRequest, TResponse>(property, request);
                                    return Task.FromResult(response);
                                });
                                return;
                            }
                        }
                        
                        throw new NotSupportedException();
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

        public ServerServiceDefinition Create(Type type)
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            var binder = new CustomServiceBinder(builder, this, _grpcObject);

            var bindServiceMethod =
                type
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(x => x.Name == "BindService")
                    .SingleOrDefault(x => x.GetParameters().Length == 2);
            if (bindServiceMethod == null)
            {
                throw new Exception("Can't find the BindService method.");
            }

            bindServiceMethod.Invoke(null, new[] {binder, (object) null});
            
            return builder.Build();
        }
    }
}