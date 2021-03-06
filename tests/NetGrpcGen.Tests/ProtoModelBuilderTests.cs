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
                    protoProperty.UpdatedEvent.Should().NotBeNull();
                    protoProperty.UpdatedEvent.Name.Should().Be($"{grpcObject.Name}{property.Name}PropertyChanged");
                    protoProperty.Getter.Should().NotBeNull();
                    protoProperty.Getter.Name.Should().Be($"GetProperty{property.Name}");
                    protoProperty.Setter.Should().NotBeNull();
                    protoProperty.Setter.Name.Should().Be($"SetProperty{property.Name}");
                }

                grpcObject.Methods.Count.Should().Be(protoObject.Methods.Count);
            
                foreach (var method in grpcObject.Methods)
                {
                    var protoMethod = protoObject.Methods.SingleOrDefault(x => x.MethodName == method.Name);
                    protoMethod.Should().NotBeNull();
                    protoMethod.MethodDescriptor.Should().NotBeNull();
                    protoMethod.MethodDescriptor.Name.Should().Be($"Invoke{method.Name}");
                }

                foreach (var ev in grpcObject.Events)
                {
                    var protoEvent = protoObject.Events.SingleOrDefault(x => x.EventName == ev.Name);
                    protoEvent.Should().NotBeNull();
                    protoEvent.MessageDescriptor.Should().NotBeNull();
                    protoEvent.MessageDescriptor.Name.Should().Be($"{grpcObject.Name}{ev.Name}Event");
                }
            }
        }
    }
}