using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using NetGrpcGen.Adapters;
using NetGrpcGen.ComponentModel;
using NetGrpcGen.Infra;
using NetGrpcGen.Model;

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

            return results.Select(x =>
            {
                var o = new GrpcObject
                {
                    Name = x.Item1.Name,
                    Attribute = x.Item2,
                    Type = x.Item1,
                    ImplementedINotify = typeof(INotifyPropertyChanged).IsAssignableFrom(x.Item1)
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
                            m.ResponseType = new GrpcType
                            {
                                Import = "google/protobuf/empty.proto",
                                TypeName = "google.protobuf.Empty"
                            };
                        }
                        else
                        {
                            if (generateParameters.Length > 1)
                            {
                                // Huh?
                                throw new NotSupportedException();
                            }
                            var returnType = generateParameters[0];
                            if (!typeof(IMessage).IsAssignableFrom(returnType))
                            {
                                throw new Exception("Invalid return type, must implement IMessage.");
                            }

                            m.ResponseType = new GrpcType
                            {
                                TypeName = returnType.Name
                            };
                        }

                        m.IsAsync = true;
                    }
                    else
                    {
                        if (m.Method.ReturnType != typeof(void))
                        {
                            if (!typeof(IMessage).IsAssignableFrom(m.Method.ReturnType))
                            {
                                throw new Exception("Invalid return type, must implement IMessage.");
                            }
                            m.ResponseType = new GrpcType
                            {
                                TypeName = m.Method.ReturnType.Name
                            };
                        }
                        else
                        {
                            m.ResponseType = new GrpcType
                            {
                                Import = "google/protobuf/empty.proto",
                                TypeName = "google.protobuf.Empty"
                            };
                        }
                    }

                    var parameters = m.Method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        throw new Exception("All methods must have at least one parameter.");
                    }
                    
                    if (parameters.Length == 1)
                    {
                        if (!typeof(IObjectMessage).IsAssignableFrom(parameters[0].ParameterType))
                        {
                            throw new Exception("Parameter must implement IObjectMessage.");
                        }

                        m.RequestType = new GrpcType
                        {
                            TypeName = parameters[0].ParameterType.Name
                        };
                    }
                    else
                    {
                        throw new Exception("Invalid number of parameters.");
                    }
                    
                    o.Methods.Add(m);
                }
                
                return o;
            }).ToList();
        }

        private GrpcType GetGrpcType(Type type)
        {
            if (type == typeof(String))
            {
                return new GrpcType
                {
                    TypeName = "string"
                };
            }

            if (typeof(IMessage).IsAssignableFrom(type))
            {
                return new GrpcType
                {
                    TypeName = type.Name
                };
            }
            
            throw new NotSupportedException("Invalid data type: " + type.FullName);
        }
    }
}