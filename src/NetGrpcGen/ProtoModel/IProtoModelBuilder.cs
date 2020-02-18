using System.Collections.Generic;
using Google.Protobuf.Reflection;

namespace NetGrpcGen.ProtoModel
{
    public interface IProtoModelBuilder
    {
        List<ProtoObjectModel> BuildObjectModels(FileDescriptor fileDescriptorProto);
    }
}