using Google.Protobuf.Reflection;

namespace NetGrpcGen.ProtoModel
{
    public class ProtoMethodModel
    {
        public ProtoObjectModel ObjectModel { get; set; }
        
        public string MethodName { get; set; }
        
        public MethodDescriptor MethodDescriptor { get; set; }
    }
}