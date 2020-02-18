using System;
using System.Collections.Generic;
using Google.Protobuf.Reflection;

namespace NetGrpcGen.ProtoModel
{
    public interface IProtoModelBuilder
    {
        List<ProtoObjectModel> BuildObjectModels(FileDescriptor serviceDescriptor);
        
        ProtoObjectModel BuildObjectModel(ServiceDescriptor serviceDescriptor);
    }
}