using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Reflection;

namespace NetGrpcGen.ProtoModel.Impl
{
    public class ProtoModelBuilder : IProtoModelBuilder
    {
        public ProtoModelBuilder()
        {
            
        }
        
        public List<ProtoObjectModel> BuildObjectModels(FileDescriptor fileDescriptor)
        {
            var result = new List<ProtoObjectModel>();
            
            // First, find the objects.
            foreach (var service in fileDescriptor.Services)
            {
                if (service.Name.EndsWith("ObjectService"))
                {
                    var objectModel = new ProtoObjectModel();
                    objectModel.ObjectName = service.Name.Substring(0, service.Name.Length - "ObjectService".Length);
                    objectModel.ServiceDescriptor = service;
                    result.Add(objectModel);
                }
            }

            // Now, find the properties.
            foreach (var objectModel in result)
            {
                foreach (var method in objectModel.ServiceDescriptor.Methods)
                {
                    if (method.Name == "Create")
                    {
                        objectModel.CreateDescriptor = method;
                        continue;
                    }

                    if (method.Name == "ListenEvents")
                    {
                        objectModel.ListEventsDescriptor = method;
                        continue;
                    }

                    if (method.Name.StartsWith("Invoke"))
                    {
                        var methodModel = new ProtoMethodModel
                        {
                            MethodName = method.Name.Substring("Invoke".Length),
                            MethodDescriptor = method
                        };
                        objectModel.Methods.Add(methodModel);
                        continue;
                    }

                    if (method.Name.StartsWith("GetProperty") || method.Name.StartsWith("SetProperty"))
                    {
                        var propertyName = method.Name.Substring("SetProperty".Length);
                        var propertyModel = objectModel.Properties.SingleOrDefault(x => x.PropertyName == propertyName);
                        if (propertyModel == null)
                        {
                            propertyModel = new ProtoPropertyModel
                            {
                                PropertyName = propertyName
                            };
                            objectModel.Properties.Add(propertyModel);
                        }

                        if (method.Name.StartsWith("GetProperty"))
                        {
                            propertyModel.Getter = method;
                        }
                        else
                        {
                            propertyModel.Setter = method;
                        }
                        
                        continue;
                    }
                    
                    throw new Exception($"Unknown method: {method.Name}");
                }
            }
            
            return result;
        }
    }
}