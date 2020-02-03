using System;
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
    public class ObjectServiceTests : BaseTests
    {
        [Fact]
        public async Task Can_create_and_finalize_object()
        {
            await WithWithObject((client, stream, instance, objectId) => Task.CompletedTask);
        }
        
        [Fact]
        public async Task Can_read_property()
        {
            await WithWithObject(async (client, stream, instance, objectId) =>
            {
                instance.Prop = "testt";
                
                // Flush the prop-changed event.
                await stream.ResponseStream.MoveNext();
                stream.ResponseStream.Current.Unpack<Test1PropChanged>();
                
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
            await WithWithObject(async (client, stream, instance, objectId) =>
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

        private async Task WithWithObject(Func<Test1ObjectService.Test1ObjectServiceClient, AsyncDuplexStreamingCall<Any, Any>, Test1, ulong, Task> action)
        {
            var instance = new Test1();
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
                Test1ObjectServiceProperty>(objectAdapter);

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