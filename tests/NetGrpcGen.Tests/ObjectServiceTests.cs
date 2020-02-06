using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;
using NetGrpcGen.Adapters;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;
using Xunit.Sdk;

namespace NetGrpcGen.Tests
{
    public class ObjectServiceTests : BaseTests
    {
        [Fact]
        public async Task Can_create_and_finalize_object()
        {
            var o = new Mock<Test1>();
            await WithWithObject(o.Object, (client, stream, instance, objectId) => Task.CompletedTask);
        }

        [Fact]
        public async Task Can_invoke_method()
        {
            var o = new Mock<Test1>();
            o.CallBase = true;
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var response = await client.TestMethodAsync(new TestMessageRequest
                {
                    ObjectId = objectId,
                    Value1 = 3,
                    Value2 = "234"
                });
                response.Value1.Should().Be(3);
                response.Value2.Should().Be("234");
                o.Verify(x => x.TestMethod(It.IsAny<TestMessageRequest>()), Times.Once());
            });
        }
        
        [Fact]
        public async Task Can_invoke_method_sync()
        {
            var o = new Mock<Test1>();
            o.CallBase = true;
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var response = await client.TestMethodSyncAsync(new TestMessageRequest
                {
                    ObjectId = objectId,
                    Value1 = 3,
                    Value2 = "234"
                });
                response.Value1.Should().Be(3);
                response.Value2.Should().Be("234");
                o.Verify(x => x.TestMethodSync(It.IsAny<TestMessageRequest>()), Times.Once());
            });
        }

        [Fact]
        public async Task Can_invoke_method_with_no_return_type()
        {
            var o = new Mock<Test1>();
            o.Setup(x => x.TestMethodWithNoResponse(It.IsAny<TestMessageRequest>()));
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var response = await client.TestMethodWithNoResponseAsync(new TestMessageRequest
                {
                    ObjectId = objectId
                });
                o.Verify(x => x.TestMethodWithNoResponse(It.IsAny<TestMessageRequest>()), Times.Once());
            });
        }
      
        private async Task WithWithObject(Test1 instance, Func<Test1ObjectService.Test1ObjectServiceClient, AsyncDuplexStreamingCall<Any, Any>, Test1, ulong, Task> action)
        {
            var objectAdapter = new Test1Adapter(instance);
            var serviceAdapter = new ObjectServiceAdapter<Test1,
                Test1CreateResponse,
                Test1StopRequest,
                Test1StopResponse>(objectAdapter, BuildDiscoveryService(typeof(Test1)).DiscoverObjects().First());

            var serverHandler = new Server
            {
                Services = {  serviceAdapter.Create(binder => { Test1ObjectService.BindService(binder, null); }) },
                Ports =
                {
                    new ServerPort("localhost", 8000, ServerCredentials.Insecure)
                }
            };
            serverHandler.Start();

            var client = new Test1ObjectService.Test1ObjectServiceClient(new Channel("localhost", 8000,
                ChannelCredentials.Insecure));

            var stream = client.Create();
            
            (await stream.ResponseStream.MoveNext()).Should().BeTrue();

            var objectId = stream.ResponseStream.Current.Unpack<Test1CreateResponse>().ObjectId;

            await action(client, stream, instance, objectId);
            
            // Finalize the object.
            await stream.RequestStream.WriteAsync(Any.Pack(new Test1StopRequest()));
            (await stream.ResponseStream.MoveNext()).Should().BeTrue();
            stream.ResponseStream.Current.Unpack<Test1StopResponse>();
            stream.Dispose();

            await serverHandler.ShutdownAsync();
        }
    }
}