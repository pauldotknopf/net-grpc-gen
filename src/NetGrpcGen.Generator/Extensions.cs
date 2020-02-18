using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace NetGrpcGen.Generator
{
    public class Extensions
    {
        public static readonly Extension<ServiceOptions, string> ServiceObjectName = new Extension<ServiceOptions, string>(
            1000,
            FieldCodec.ForString(WireFormat.MakeTag(1000,
                WireFormat.WireType.LengthDelimited)));
        
        public static readonly Extension<MethodOptions, bool> MethodCreate = new Extension<MethodOptions, bool>(
            1000,
            FieldCodec.ForBool(WireFormat.MakeTag(1000,
                WireFormat.WireType.Varint)));
        
        public static readonly Extension<MethodOptions, bool> MethodEventListener = new Extension<MethodOptions, bool>(
            1001,
            FieldCodec.ForBool(WireFormat.MakeTag(1001,
                WireFormat.WireType.Varint)));
        
        public static readonly Extension<MethodOptions, string> MethodName = new Extension<MethodOptions, string>(
            1002,
            FieldCodec.ForString(WireFormat.MakeTag(1002,
                WireFormat.WireType.LengthDelimited)));
        
        public static readonly Extension<MethodOptions, bool> MethodSync = new Extension<MethodOptions, bool>(
            1003,
            FieldCodec.ForBool(WireFormat.MakeTag(1003,
                WireFormat.WireType.Varint)));
        
        public static readonly Extension<MethodOptions, string> MethodPropName = new Extension<MethodOptions, string>(
            1004,
            FieldCodec.ForString(WireFormat.MakeTag(1004,
                WireFormat.WireType.LengthDelimited)));
        
        public static readonly Extension<MethodOptions, bool> MethodPropGet = new Extension<MethodOptions, bool>(
            1005,
            FieldCodec.ForBool(WireFormat.MakeTag(1005,
                WireFormat.WireType.Varint)));
        
        public static readonly Extension<MethodOptions, bool> MethodPropSet = new Extension<MethodOptions, bool>(
            1006,
            FieldCodec.ForBool(WireFormat.MakeTag(1006,
                WireFormat.WireType.Varint)));

        public static readonly Extension<MessageOptions, string> MessageObjectName = new Extension<MessageOptions, string>(
            1000,
            FieldCodec.ForString(WireFormat.MakeTag(1000,
                WireFormat.WireType.LengthDelimited)));
        
        public static readonly Extension<MessageOptions, string> MessageEventName = new Extension<MessageOptions, string>(
            1001,
            FieldCodec.ForString(WireFormat.MakeTag(1001,
                WireFormat.WireType.LengthDelimited)));
        
        public static readonly Extension<MessageOptions, string> MessageForProp = new Extension<MessageOptions, string>(
            1002,
            FieldCodec.ForString(WireFormat.MakeTag(1002,
                WireFormat.WireType.LengthDelimited)));
    }
}