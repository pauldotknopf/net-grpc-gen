using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

                var propChangedEventRegex = new Regex($@"{objectModel.ObjectName}(.*)PropertyChanged");
                var eventRegex = new Regex($@"{objectModel.ObjectName}(.*)Event");
                
                foreach (var messageDescriptor in fileDescriptor.MessageTypes)
                {
                    if (messageDescriptor.Name == $"{objectModel.ObjectName}CreateResponse")
                    {
                        objectModel.CreateResponseDescriptor = messageDescriptor;
                        continue;
                    }

                    if (messageDescriptor.Name == $"{objectModel.ObjectName}StopRequest")
                    {
                        objectModel.StopRequestDescriptor = messageDescriptor;
                        continue;
                    }

                    if (messageDescriptor.Name == $"{objectModel.ObjectName}StopResponse")
                    {
                        objectModel.StopResponseDescriptor = messageDescriptor;
                        continue;
                    }

                    var match = propChangedEventRegex.Match(messageDescriptor.Name);
                    if (match.Success)
                    {
                        var propertyName = match.Groups[1].Value;
                        var propertyModel = objectModel.Properties.Single(x => x.PropertyName == propertyName);
                        propertyModel.UpdatedEvent = messageDescriptor;
                        continue;
                    }

                    match = eventRegex.Match(messageDescriptor.Name);
                    if (match.Success)
                    {
                        var eventName = match.Groups[1].Value;
                        var eventModel = new ProtoEventModel();
                        eventModel.EventName = eventName;
                        eventModel.MessageDescriptor = messageDescriptor;
                        objectModel.Events.Add(eventModel);
                    }
                }
            }

            return result;
        }
    }
}