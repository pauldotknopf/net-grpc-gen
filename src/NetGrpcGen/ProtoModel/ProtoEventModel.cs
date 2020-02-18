using Google.Protobuf.Reflection;

namespace NetGrpcGen.ProtoModel
{
    public class ProtoEventModel
    {
        public string EventName { get; set; }
        
        public MessageDescriptor MessageDescriptor { get; set; }
    }
}