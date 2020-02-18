using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NetGrpcGen.Adapters;
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
            var objectAdapter = new Test1Adapter(null);
            var serviceAdapter = new ObjectServiceAdapter<Test1,
                Test1CreateResponse,
                Test1StopRequest,
                Test1StopResponse>(objectAdapter, BuildDiscoveryService(typeof(Test1)).DiscoverObjects().First());

            var serverHandler = new Server
            {
                Services = {  serviceAdapter.Create(typeof(Test1ObjectService)) },
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