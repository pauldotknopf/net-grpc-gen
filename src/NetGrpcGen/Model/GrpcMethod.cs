using System.Collections.Generic;
using System.Reflection;
using NetGrpcGen.ComponentModel;

namespace NetGrpcGen.Model
{
    public class GrpcMethod
    {
        public GrpcMethod()
        {
            Arguments = new List<GrpcArgument>();
        }
        
        public string Name { get; set; }
        
        public GrpcMethodAttribute Attribute { get; set; }
        
        public GrpcObject GrpcObject { get; set; }
        
        public MethodInfo Method { get; set; }
        
        public GrpcDataType? ReturnType { get; set; }
        
        public List<GrpcArgument> Arguments { get; set; }
    }
}