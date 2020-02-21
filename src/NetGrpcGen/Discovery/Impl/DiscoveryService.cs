using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using NetGrpcGen.Adapters;
using NetGrpcGen.ComponentModel;
using NetGrpcGen.Infra;
using NetGrpcGen.Model;
using Type = System.Type;

namespace NetGrpcGen.Discovery.Impl
{
    public class DiscoveryService : IDiscoveryService
    {
        private readonly IAttributeFinder _attributeFinder;

        public DiscoveryService(IAttributeFinder attributeFinder)
        {
            _attributeFinder = attributeFinder;
        }
        
        public List<GrpcObject> DiscoverObjects()
        {
            var results = _attributeFinder.FindObjectsWithAttribute<GrpcObjectAttribute>();
            return results.Select(x => BuildObject(x.Item2, x.Item1)).ToList();
        }

        public GrpcObject BuildObject(Type type)
        {
            var attribute = type.GetCustomAttributes(typeof(GrpcObjectAttribute), false);
            
            if (attribute.Length > 0)
            {
                return BuildObject(attribute[0] as GrpcObjectAttribute, type);
            }

            return null;
        }

        private GrpcObject BuildObject(GrpcObjectAttribute attribute, Type type)
        {
            var o = new GrpcObject
            {
                Name = type.Name,
                Attribute = attribute,
                Type = type,
                ImplementedINotify = typeof(INotifyPropertyChanged).IsAssignableFrom(type)
            };

            foreach (var prop in _attributeFinder.FindPropertiesWithAttribute<GrpcPropertyAttribute>(o.Type))
            {
                var p = new GrpcProperty
                {
                    Name = prop.Item1.Name,
                    Attribute = prop.Item2,
                    Property = prop.Item1,
                    GrpcObject = o
                };

                if (!p.Property.CanRead)
                {
                    throw new Exception("All properties must support reading.");
                }

                p.CanWrite = p.Property.CanWrite;
                
                p.DataType = GetGrpcType(p.Property.PropertyType);
                
                o.Properties.Add(p);
            }

            foreach (var method in _attributeFinder.FindMethodsWithAttribute<GrpcMethodAttribute>(o.Type))
            {
                var m = new GrpcMethod
                {
                    Name = method.Item1.Name,
                    Attribute = method.Item2,
                    Method = method.Item1,
                    GrpcObject = o
                };

                if (typeof(Task).IsAssignableFrom(m.Method.ReturnType))
                {
                    // This is an async method.
                    var generateParameters = m.Method.ReturnType.GetGenericArguments();
                    if (generateParameters.Length == 0)
                    {
                        m.ResponseType = null;
                    }
                    else
                    {
                        if (generateParameters.Length > 1)
                        {
                            // Huh?
                            throw new NotSupportedException();
                        }
                        var returnType = generateParameters[0];
                        m.ResponseType = GetGrpcType(returnType);
                    }

                    m.IsAsync = true;
                }
                else
                {
                    if (m.Method.ReturnType != typeof(void))
                    {
                        m.ResponseType = GetGrpcType(m.Method.ReturnType);
                    }
                    else
                    {
                        m.ResponseType = null;
                    }
                }

                var parameters = m.Method.GetParameters();
             
                if (parameters.Length == 1)
                {
                    m.RequestType = GetGrpcType(parameters[0].ParameterType);
                }
                else if(parameters.Length == 0)
                {
                    m.RequestType = null;
                }
                else
                {
                    throw new Exception("Invalid number of parameters.");
                }
                
                o.Methods.Add(m);
            }

            foreach (var _ in _attributeFinder.FindEventsWithAttribute<GrpcEventAttribute>(o.Type))
            {
                var e = new GrpcEvent
                {
                    Name = _.Item1.Name,
                    Attribute = _.Item2,
                    Event = _.Item1,
                    GrpcObject = o
                };

                var genericArguments = e.Event.EventHandlerType.GetGenericArguments();
                
                if (genericArguments.Length == 1)
                {
                    if (e.Event.EventHandlerType.GetGenericTypeDefinition() != typeof(GrpcObjectEventDelegate<>))
                    {
                        throw new Exception("Invalid event handler type.");
                    }

                    e.DataType = GetGrpcType(genericArguments[0]);
                
                    o.Events.Add(e);
                } else if (genericArguments.Length == 0)
                {
                    if (e.Event.EventHandlerType != typeof(GrpcObjectEventDelegate))
                    {
                        throw new Exception("Invalid event handler type.");
                    }
                    
                    o.Events.Add(e);
                }
                else
                {
                    throw new Exception("Invalid event handler type.");
                }
            }

            return o;
        }
        
        private GrpcType GetGrpcType(Type type)
        {
            if (type == typeof(String))
            {
                return GetGrpcType(typeof(StringValue));
            }

            if (type == typeof(int))
            {
                return new GrpcType
                {
                    TypeName = "int32"
                };
            }

            if (typeof(IMessage).IsAssignableFrom(type))
            {
                var descriptor = GetDescriptor(type);
                return new GrpcType
                {
                    TypeName = descriptor.FullName,
                    Import = descriptor.File.Name
                };
            }
            
            throw new NotSupportedException("Invalid data type: " + type.FullName);
        }

        private MessageDescriptor GetDescriptor(Type type)
        {
            var message = Activator.CreateInstance(type) as IMessage;
            if (message == null)
            {
                throw new Exception($"The type {type.Name} doesn't implement IMessage.");
            }
            return message.Descriptor;
        }
    }
}