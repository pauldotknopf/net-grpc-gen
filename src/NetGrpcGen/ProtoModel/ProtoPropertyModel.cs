using Google.Protobuf.Reflection;

namespace NetGrpcGen.ProtoModel
{
    public class ProtoPropertyModel
    {
        public string PropertyName { get; set; }
        
        public MethodDescriptor Setter { get; set; }
        
        public MethodDescriptor Getter { get; set; }
        
        public MessageDescriptor UpdatedEvent { get; set; }
    }
}