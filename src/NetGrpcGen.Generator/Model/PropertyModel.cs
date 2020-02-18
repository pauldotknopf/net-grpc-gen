using Google.Protobuf.Reflection;

namespace NetGrpcGen.Generator.Model
{
    public class PropertyModel
    {
        public string PropertyName { get; set; }
        
        public MethodDescriptorProto Getter { get; set; }
        
        public MethodDescriptorProto Setter { get; set; }
    }
}