using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NetGrpcGen.Adapters;
using NetGrpcGen.Adapters.Impl;
using NetGrpcGen.Discovery.Impl;
using NetGrpcGen.Infra.Impl;
using NetGrpcGen.ProtoModel.Impl;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests
{
    public class IntegrationTests : BaseTests
    {
        [Fact]
        public async Task Start_server()
        {
            var serviceAdapter = new ObjectServiceAdapter<Test1>(
                new ProtoModelBuilder(),
                new DiscoveryService(new AttributeFinder(new TypeFinder())),
                new TypeCreator<Test1>(),
                typeof(Test1ObjectService));

            var serverHandler = new Server
            {
                Services = {  serviceAdapter.Create() },
                Ports =
                {
                    new ServerPort("localhost", 8000, ServerCredentials.Insecure)
                }
            };
            serverHandler.Start();

            await Task.Delay(Timeout.Infinite);
            
            await serverHandler.ShutdownAsync();
        }
    }
}