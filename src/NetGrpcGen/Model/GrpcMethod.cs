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
    }
}