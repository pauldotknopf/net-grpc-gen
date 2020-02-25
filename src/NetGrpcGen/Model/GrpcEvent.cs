using System;
using System.Reflection;
using NetGrpcGen.ComponentModel;

namespace NetGrpcGen.Model
{
    public class GrpcEvent
    {
        public string Name { get; set; }
        
        public GrpcEventAttribute Attribute { get; set; }
        
        public GrpcObject GrpcObject { get; set; }
        
        public EventInfo Event { get; set; }
        
        public GrpcType DataType { get; set; }
        
        public Type ClrDataType { get; set; }
    }
}