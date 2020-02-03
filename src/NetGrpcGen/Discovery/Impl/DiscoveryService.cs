using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                    
                    p.DataType = GetDataType(p.Property.PropertyType);
                    
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

                    if (m.Method.ReturnType != typeof(void))
                    {
                        m.ReturnType = GetDataType(m.Method.ReturnType);
                    }

                    foreach (var arg in m.Method.GetParameters())
                    {
                        m.Arguments.Add(new GrpcArgument
                        {
                            Name = arg.Name,
                            Type = arg.ParameterType,
                            DataType = GetDataType(arg.ParameterType)
                        });
                    }
                    
                    o.Methods.Add(m);
                }
                
                return o;
            }).ToList();
        }

        private GrpcDataType GetDataType(Type type)
        {
            if (type == typeof(String))
            {
                return GrpcDataType.String;
            }

            if (type == typeof(int))
            {
                return GrpcDataType.Int32;
            }

            if (type.IsClass)
            {
                return GrpcDataType.Complex;
            }
            
            throw new NotSupportedException("Invalid data type: " + type.FullName);
        }
    }
}