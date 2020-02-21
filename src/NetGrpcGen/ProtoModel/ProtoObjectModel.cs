using System;
using System.Collections.Generic;
using Google.Protobuf.Reflection;

namespace NetGrpcGen.ProtoModel
{
    public class ProtoObjectModel
    {
        public string ObjectName { get; set; }
        
        public ServiceDescriptor ServiceDescriptor { get; set; }
        
        public MethodDescriptor CreateDescriptor { get; set; }
        
        public MessageDescriptor CreateResponseDescriptor { get; set; }
        
        public MessageDescriptor StopRequestDescriptor { get; set; }
        
        public MessageDescriptor StopResponseDescriptor { get; set; }
        
        public MethodDescriptor ListEventsDescriptor { get; set; }
        
        public List<ProtoMethodModel> Methods { get; set; } = new List<ProtoMethodModel>();
        
        public List<ProtoPropertyModel> Properties { get; set; } = new List<ProtoPropertyModel>();
        
        public List<ProtoEventModel> Events { get; set; } = new List<ProtoEventModel>();
    }
}