using Google.Protobuf.Reflection;

namespace NetGrpcGen.ProtoModel
{
    public class ProtoMethodModel
    {
        public string MethodName { get; set; }
        
        public MethodDescriptor MethodDescriptor { get; set; }
    }
}