using Google.Protobuf;

namespace NetGrpcGen.Adapters
{
    public interface IPropertyMessage<TPropertyEnum> : IMessage
    {
        TPropertyEnum Prop { get; set; }
    }
}