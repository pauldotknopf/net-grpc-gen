using Google.Protobuf;

namespace NetGrpcGen.Adapters
{
    public interface IObjectMessage : IMessage
    {
        ulong ObjectId { get; set; }
    }
}