using Google.Protobuf.Reflection;

namespace NetGrpcGen.Generator.Model
{
    public class MethodModel
    {
        public string Name { get; set; }
        
        public bool Sync { get; set; }
        
        public MethodDescriptorProto InvokeMethod { get; set; }
    }
}