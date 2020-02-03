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
                    
                    if (p.Property.PropertyType == typeof(string))
                    {
                        p.PropertyType = GrpcPropertyType.String;
                    }
                    else
                    {
                        throw new Exception($"Invalid property type {p.Property.PropertyType.Name} for {p.Name} on {o.Name}");
                    }
                    
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
                    
                    o.Methods.Add(m);
                }
                
                return o;
            }).ToList();
        }
    }
}