using System.Linq;
using FluentAssertions;
using NetGrpcGen.ProtoModel;
using NetGrpcGen.ProtoModel.Impl;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests
{
    public class ProtoModelBuilderTests : BaseTests
    {
        private readonly IProtoModelBuilder _protoModelBuilder;
        
        public ProtoModelBuilderTests()
        {
            _protoModelBuilder = new ProtoModelBuilder();
        }
        
        [Fact]
        public void Can_build_proto_object_model()
        {
            var grpcObjects = base.BuildDiscoveryService(typeof(Test1), typeof(Test2)).DiscoverObjects();
            var protoObjects = _protoModelBuilder.BuildObjectModels(Test2ObjectService.Descriptor.File);
            
            grpcObjects.Should().HaveCount(2);
            protoObjects.Should().HaveCount(2);

            foreach (var protoObject in protoObjects)
            {
                var grpcObject = grpcObjects.SingleOrDefault(x => x.Name == protoObject.ObjectName);
                grpcObject.Should().NotBeNull();
                
                grpcObject.Properties.Count.Should().Be(protoObject.Properties.Count);
            
                foreach (var property in grpcObject.Properties)
                {
                    var protoProperty = protoObject.Properties.SingleOrDefault(x => x.PropertyName == property.Name);
                    protoProperty.Should().NotBeNull();
                }

                grpcObject.Methods.Count.Should().Be(protoObject.Methods.Count);
            
                foreach (var method in grpcObject.Methods)
                {
                    var protoMethod = protoObject.Methods.SingleOrDefault(x => x.MethodName == method.Name);
                    protoMethod.Should().NotBeNull();
                }

                foreach (var ev in grpcObject.Events)
                {
                    var protoEvent = protoObject.Events.SingleOrDefault(x => x.EventName == ev.Name);
                    protoEvent.Should().NotBeNull();
                }
            }
        }
    }
}