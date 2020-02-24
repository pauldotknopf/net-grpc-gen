using System;
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

namespace NetGrpcGen.Tests.TypeTests
{
    public class BaseTypeTests : BaseTests
    {
        protected async Task WithWithObject(TestTypes instance, Func<TestTypesObjectService.TestTypesObjectServiceClient, TestTypes, ulong, Task> action)
        {
            var serviceAdapter = new ObjectServiceAdapter<TestTypes>(new ProtoModelBuilder(),
                new DiscoveryService(new AttributeFinder(new TypeFinder())),
                new FixedTypeCreator<TestTypes>(instance),
                typeof(TestTypesObjectService));
            
            var serverHandler = new Server
            {
                Services = {  serviceAdapter.Create() },
                Ports =
                {
                    new ServerPort("localhost", 8000, ServerCredentials.Insecure)
                }
            };
            serverHandler.Start();

            var client = new TestTypesObjectService.TestTypesObjectServiceClient(new Channel("localhost", 8000,
                ChannelCredentials.Insecure));

            var stream = client.Create(new TestTypesCreateRequest());
            
            (await stream.ResponseStream.MoveNext()).Should().BeTrue();
            
            var objectId = stream.ResponseStream.Current.ObjectId;

            await action(client, instance, objectId);
            
            // Finalize the object.
            stream.Dispose();
            
            await serverHandler.ShutdownAsync();
        }
    }
}