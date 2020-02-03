using System;
using System.Linq;
using Moq;
using NetGrpcGen.Discovery;
using NetGrpcGen.Discovery.Impl;
using NetGrpcGen.Infra;
using NetGrpcGen.Infra.Impl;

namespace NetGrpcGen.Tests
{
    public class BaseTests
    {
        protected IDiscoveryService BuildDiscoveryService(params Type[] types)
        {
            var mock = new Mock<ITypeFinder>();
            mock.Setup(x => x.GetTypes()).Returns(types.ToList());
            return new DiscoveryService(new AttributeFinder(mock.Object));
        }
    }
}