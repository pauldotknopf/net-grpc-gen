using Humanizer;
using NetGrpcGen.ProtoModel;

namespace NetGrpcGen.Generator
{
    public static class EventModelExtensions
    {
        public static string GetEventName(this ProtoEventModel val)
        {
            return val.EventName.Camelize();
        }
    }
}