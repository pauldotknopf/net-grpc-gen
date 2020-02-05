using System.Collections.Generic;
using System.Reflection;
using NetGrpcGen.ComponentModel;

namespace NetGrpcGen.Model
{
    public class GrpcMethod
    {
        public string Name { get; set; }
        
        public GrpcMethodAttribute Attribute { get; set; }
        
        public GrpcObject GrpcObject { get; set; }
        
        public MethodInfo Method { get; set; }
        
        public bool IsAsync { get; set; }
        
        public GrpcType RequestType { get; set; }
        
        public GrpcType ResponseType { get; set; }
    }
}