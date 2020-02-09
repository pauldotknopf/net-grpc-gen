using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;
using Custom.Types;
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
        public async Task Can_invoke_method()
        {
            var o = new Mock<Test1>();
            o.CallBase = true;
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var response = await client.TestMethodAsync(new Test1TestMethodMethodRequest
                {
                    ObjectId = objectId,
                    Value = new TestMessageRequest
                    {
                        Value1 = 3,
                        Value2 = "234"
                    }
                });
                response.Value.Value1.Should().Be(3);
                response.Value.Value2.Should().Be("234");
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
                var response = await client.TestMethodSyncAsync(new Test1TestMethodSyncMethodRequest
                {
                    ObjectId = objectId,
                    Value = new TestMessageRequest
                    {
                        Value1 = 3,
                        Value2 = "234"
                    }
                });
                response.Value.Value1.Should().Be(3);
                response.Value.Value2.Should().Be("234");
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
                await client.TestMethodWithNoResponseAsync(new Test1TestMethodWithNoResponseMethodRequest
                {
                    ObjectId = objectId,
                    Value = new TestMessageRequest()
                });
                o.Verify(x => x.TestMethodWithNoResponse(It.IsAny<TestMessageRequest>()), Times.Once());
            });
        }

        [Fact]
        public async Task Can_read_property()
        {
            var o = new Mock<Test1>();
            o.SetupGet(x => x.PropString).Returns("sssdsd");
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var response = await client.GetPropertyPropStringAsync(new Test1PropStringGetRequest
                {
                    ObjectId = objectId
                });
                response.Value.Should().Be("sssdsd");
                o.VerifyGet(x => x.PropString, Times.Once);
            });
        }

        [Fact]
        public async Task Can_set_property()
        {
            var o = new Mock<Test1>();
            o.SetupSet(x => x.PropString = "TTT");
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                await client.SetPropertyPropStringAsync(new Test1PropStringSetRequest
                {
                    ObjectId = objectId,
                    Value = "TTT"
                });
                o.VerifySet(x => x.PropString = "TTT", Times.Once);
            });
        }
        
        [Fact]
        public async Task Can_raise_prop_changed()
        {
            var o = new Mock<Test1>();
            o.SetupGet(x => x.PropString).Returns("sdfxcvs");
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var eventStream = client.ListenEvents(new Test1ListenEventStream
                {
                    ObjectId = objectId
                });
                
                o.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs("PropString"));

                await eventStream.ResponseStream.MoveNext();
                var propChanged = eventStream.ResponseStream.Current.Unpack<Test1PropStringPropertyChanged>();
                propChanged.Value.Should().Be("sdfxcvs");
                propChanged.ObjectId.Should().Be(objectId);
                
                eventStream.Dispose();
            });
        }
        
        [Fact]
        public async Task Can_raise_prop_changed_on_complex_type()
        {
            var o = new Mock<Test1>();
            o.SetupGet(x => x.PropComplex).Returns(new TestMessageResponse
            {
                Value1 = 78,
                Value2 = "sss"
            });
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var eventStream = client.ListenEvents(new Test1ListenEventStream
                {
                    ObjectId = objectId
                });
                
                o.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs("PropComplex"));

                await eventStream.ResponseStream.MoveNext();
                var propChanged = eventStream.ResponseStream.Current.Unpack<Test1PropComplexPropertyChanged>();
                propChanged.Value.Value1.Should().Be(78);
                propChanged.Value.Value2.Should().Be("sss");
                propChanged.ObjectId.Should().Be(objectId);
                
                eventStream.Dispose();
            });
        }

        [Fact]
        public async Task Can_listen_to_event()
        {
            var o = new Mock<Test1>();
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var eventStream = client.ListenEvents(new Test1ListenEventStream
                {
                    ObjectId = objectId
                });
                
                o.Raise(x => x.TestEvent += null, "tettsedf");

                await eventStream.ResponseStream.MoveNext();
                var eventTest = eventStream.ResponseStream.Current.Unpack<Test1TestEventEvent>();
                eventTest.Value.Should().Be("tettsedf");
                eventTest.ObjectId.Should().Be(objectId);

                eventStream.Dispose();
            });
        }
        
        [Fact]
        public async Task Can_listen_to_event_with_complex_data()
        {
            var o = new Mock<Test1>();
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var eventStream = client.ListenEvents(new Test1ListenEventStream
                {
                    ObjectId = objectId
                });
                
                o.Raise(x => x.TestEventComplex += null, new TestMessageResponse
                {
                    Value1 = 45655674,
                    Value2 = ",,,"
                });

                await eventStream.ResponseStream.MoveNext();
                var eventTestComplex = eventStream.ResponseStream.Current.Unpack<Test1TestEventComplexEvent>();
                eventTestComplex.Value.Value1.Should().Be(45655674);
                eventTestComplex.Value.Value2.Should().Be(",,,");
                eventTestComplex.ObjectId.Should().Be(objectId);
                
                eventStream.Dispose();
            });
        }

        [Fact]
        public async Task Can_listen_to_event_with_no_data()
        {
            var o = new Mock<Test1>();
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                var eventStream = client.ListenEvents(new Test1ListenEventStream
                {
                    ObjectId = objectId
                });
                
                o.Raise(x => x.TestEventNoData += null);

                await eventStream.ResponseStream.MoveNext();
                var eventTestComplex = eventStream.ResponseStream.Current.Unpack<Test1TestEventNoDataEvent>();
                eventTestComplex.ObjectId.Should().Be(objectId);
                
                eventStream.Dispose();
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
                Services = {  serviceAdapter.Create(typeof(Test1ObjectService)) },
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