using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            foreach (var service in fileDescriptor.Services)
            {
                var model = BuildObjectModel(service);
                if (model != null)
                {
                    result.Add(model);
                }
            }
            
            return result;
        }

        public ProtoObjectModel BuildObjectModel(ServiceDescriptor serviceDescriptor)
        {
            ProtoObjectModel result = null;

            if (!serviceDescriptor.Name.EndsWith("ObjectService"))
            {
                return result;
            }

            result = new ProtoObjectModel
            {
                ObjectName =
                    serviceDescriptor.Name.Substring(0, serviceDescriptor.Name.Length - "ObjectService".Length),
                ServiceDescriptor = serviceDescriptor
            };

            foreach (var method in result.ServiceDescriptor.Methods)
            {
                if (method.Name == "Create")
                {
                    result.CreateDescriptor = method;
                    continue;
                }

                if (method.Name == "ListenEvents")
                {
                    result.ListEventsDescriptor = method;
                    continue;
                }

                if (method.Name.StartsWith("Invoke"))
                {
                    var methodModel = new ProtoMethodModel
                    {
                        MethodName = method.Name.Substring("Invoke".Length),
                        MethodDescriptor = method
                    };
                    result.Methods.Add(methodModel);
                    continue;
                }

                if (method.Name.StartsWith("GetProperty") || method.Name.StartsWith("SetProperty"))
                {
                    var propertyName = method.Name.Substring("SetProperty".Length);
                    var propertyModel = result.Properties.SingleOrDefault(x => x.PropertyName == propertyName);
                    if (propertyModel == null)
                    {
                        propertyModel = new ProtoPropertyModel
                        {
                            PropertyName = propertyName
                        };
                        result.Properties.Add(propertyModel);
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

            var propChangedEventRegex = new Regex($@"{result.ObjectName}(.*)PropertyChanged");
            var eventRegex = new Regex($@"{result.ObjectName}(.*)Event");
                
            foreach (var messageDescriptor in result.ServiceDescriptor.File.MessageTypes)
            {
                if (messageDescriptor.Name == $"{result.ObjectName}CreateResponse")
                {
                    result.CreateResponseDescriptor = messageDescriptor;
                    continue;
                }

                if (messageDescriptor.Name == $"{result.ObjectName}StopRequest")
                {
                    result.StopRequestDescriptor = messageDescriptor;
                    continue;
                }

                if (messageDescriptor.Name == $"{result.ObjectName}StopResponse")
                {
                    result.StopResponseDescriptor = messageDescriptor;
                    continue;
                }

                var match = propChangedEventRegex.Match(messageDescriptor.Name);
                if (match.Success)
                {
                    var propertyName = match.Groups[1].Value;
                    var propertyModel = result.Properties.Single(x => x.PropertyName == propertyName);
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
                    result.Events.Add(eventModel);
                }
            }

            return result;
        }
    }
}