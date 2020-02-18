using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Reflection;

namespace NetGrpcGen.Generator.Model
{
    public class ModelBuilder
    {
        public static List<ObjectModel> BuildObjectModels(FileDescriptorProto fileDescriptorProto)
        {
            var result = new List<ObjectModel>();

            foreach (var service in fileDescriptorProto.Service)
            {
                if (!service.HasOptions)
                {
                    continue;
                }

                if (!service.Options.HasExtension(Extensions.ServiceObjectName))
                {
                    continue;
                }

                var objectName = service.Options.GetExtension(Extensions.ServiceObjectName);
                if (string.IsNullOrEmpty(objectName))
                {
                    throw new Exception("The object name is missing a valid value.");
                }
                
                var objectModel = new ObjectModel();
                objectModel.ObjectName = objectName;
                result.Add(objectModel);

                foreach (var method in service.Method)
                {
                    if (!method.HasOptions)
                    {
                        continue;
                    }

                    if (method.Options.HasExtension(Extensions.MethodCreate) && method.Options.GetExtension(Extensions.MethodCreate))
                    {
                        objectModel.CreateMethod = method;
                        continue;
                    }

                    if (method.Options.HasExtension(Extensions.MethodEventListener) &&
                        method.Options.GetExtension(Extensions.MethodEventListener))
                    {
                        objectModel.EventListener = method;
                        continue;
                    }
                    
                    if(method.Options.HasExtension(Extensions.MethodName))
                    {
                        var methodName = method.Options.GetExtension(Extensions.MethodName);
                        if (string.IsNullOrEmpty(methodName))
                        {
                            throw new Exception("Invalid method name.");
                        }

                        var sync = false;
                        if (method.Options.GetExtension(Extensions.MethodSync))
                        {
                            sync = method.Options.GetExtension(Extensions.MethodSync);
                        }                        
                        objectModel.Methods.Add(new MethodModel
                        {
                            Name = methodName,
                            Sync = sync,
                            InvokeMethod = method
                        });
                        
                        continue;
                    }

                    if (method.Options.HasExtension(Extensions.MethodPropName))
                    {
                        var propName = method.Options.GetExtension(Extensions.MethodPropName);
                        if (string.IsNullOrEmpty(propName))
                        {
                            throw new Exception("Invalid prop name.");
                        }

                        var propertyModel = objectModel.Properties.SingleOrDefault(x => x.PropertyName == propName);
                        if (propertyModel == null)
                        {
                            propertyModel = new PropertyModel {PropertyName = propName};
                            objectModel.Properties.Add(propertyModel);
                        }
                        
                        if (method.Options.HasExtension(Extensions.MethodPropGet) &&
                            method.Options.GetExtension(Extensions.MethodPropGet))
                        {
                            propertyModel.Getter = method;
                            continue;
                        }
                        
                        if (method.Options.HasExtension(Extensions.MethodPropSet) &&
                            method.Options.GetExtension(Extensions.MethodPropSet))
                        {
                            propertyModel.Setter = method;
                            continue;
                        }
                        
                        throw new Exception("The method must be defined as a setter or getter.");
                    }
                }
            }

            foreach (var message in fileDescriptorProto.MessageType)
            {
                
            }

            return result;
        }
    }
}