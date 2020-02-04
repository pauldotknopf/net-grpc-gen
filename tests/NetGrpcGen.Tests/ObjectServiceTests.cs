using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;
using NetGrpcGen.Adapters;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

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
        public async Task Can_read_property()
        {
            var o = new Mock<Test1>();
            o.SetupGet(x => x.Prop).Returns("testt");
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var result = await client.GetPropertyAsync(new Test1GetPropRequest
                {
                    Prop = Test1ObjectServiceProperty.Prop,
                    ObjectId = objectId
                });

                result.Prop.Should().Be(Test1ObjectServiceProperty.Prop);
                result.ObjectId.Should().Be(objectId);
                result.ValueCase.Should().Be(Test1GetPropResponse.ValueOneofCase.Str);
                result.Str.Should().Be("testt");
            });
        }

        [Fact]
        public async Task Can_set_property()
        {
            var o = new Mock<Test1>();
            o.CallBase = true;
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var result = await client.SetPropertyAsync(new Test1SetPropRequest
                {
                    Prop = Test1ObjectServiceProperty.Prop,
                    ObjectId = objectId,
                    Str = "testsdfsdttt"
                });

                result.Prop.Should().Be(Test1ObjectServiceProperty.Prop);
                result.ObjectId.Should().Be(objectId);
                instance.Prop.Should().Be("testsdfsdttt");
                
                // Let's make sure we can get the prop changed event.
                await stream.ResponseStream.MoveNext();
                var propChanged = stream.ResponseStream.Current.Unpack<Test1PropChanged>();
                propChanged.Prop.Should().Be(Test1ObjectServiceProperty.Prop);
                propChanged.ObjectId.Should().Be(objectId);
                propChanged.Str.Should().Be("testsdfsdttt");
            });
        }
        
        [Fact]
        public async Task Can_invoke_method()
        {
            var o = new Mock<Test1>();
            o.Setup(x => x.Method1());
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
                {
                    await client.Method1Async(new Method1Request
                    {
                        ObjectId = objectId
                    });
                    o.Verify(x => x.Method1(), Times.Once);
                });
        }
        
        [Fact]
        public async Task Can_invoke_method_with_return_type()
        {
            var o = new Mock<Test1>();
            o.Setup(x => x.MethodWithReturnInt()).Returns(5);
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var response = await client.MethodWithReturnIntAsync(new MethodWithReturnIntRequest
                {
                    ObjectId = objectId
                });
                response.Response.Should().Be(5);
                o.Verify(x => x.MethodWithReturnInt(), Times.Once);
            });
        }
        
        [Fact]
        public async Task Can_invoke_async_method()
        {
            var o = new Mock<Test1>();
            o.Setup(x => x.MethodWithReturnIntA()).Returns(Task.FromResult(20));
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var response = await client.MethodWithReturnIntAAsync(new MethodWithReturnIntARequest()
                {
                    ObjectId = objectId
                });
                response.Response.Should().Be(20);
                o.Verify(x => x.MethodWithReturnIntA(), Times.Once);
            });
        }

        private async Task WithWithObject(Test1 instance, Func<Test1ObjectService.Test1ObjectServiceClient, AsyncDuplexStreamingCall<Any, Any>, Test1, ulong, Task> action)
        {
            var objectAdapter = new Test1Adapter(instance);
            var serviceAdapter = new ObjectServiceAdapter<Test1,
                Test1GetPropRequest,
                Test1GetPropResponse,
                Test1SetPropRequest,
                Test1SetPropResponse,
                Test1PropChanged,
                Test1CreateResponse,
                Test1StopRequest,
                Test1StopResponse,
                Test1ObjectServiceProperty>(objectAdapter, BuildDiscoveryService(typeof(Test1)).DiscoverObjects().First());

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