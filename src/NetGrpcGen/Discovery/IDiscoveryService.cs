using System.Collections.Generic;
using NetGrpcGen.Model;

namespace NetGrpcGen.Discovery
{
    public interface IDiscoveryService
    {
        List<GrpcObject> DiscoverObjects();
    }
}