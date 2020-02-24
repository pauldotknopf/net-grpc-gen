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
        
        public MethodDescriptor ListenEventsDescriptor { get; set; }
        
        public List<ProtoMethodModel> Methods { get; set; } = new List<ProtoMethodModel>();
        
        public List<ProtoPropertyModel> Properties { get; set; } = new List<ProtoPropertyModel>();
        
        public List<ProtoEventModel> Events { get; set; } = new List<ProtoEventModel>();
    }
}