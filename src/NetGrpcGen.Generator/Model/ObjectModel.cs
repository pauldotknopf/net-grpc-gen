using System.Collections.Generic;
using Google.Protobuf.Reflection;

namespace NetGrpcGen.Generator.Model
{
    public class ObjectModel
    {
        public string ObjectName { get; set; }
        
        public MethodDescriptorProto CreateMethod { get; set; }
        
        public MethodDescriptorProto EventListener { get; set; }
        
        public List<EventModel> Events { get; set; } = new List<EventModel>();
        
        public List<MethodModel> Methods { get; set; } = new List<MethodModel>();
        
        public List<PropertyModel> Properties { get; set; } = new List<PropertyModel>();
    }
}