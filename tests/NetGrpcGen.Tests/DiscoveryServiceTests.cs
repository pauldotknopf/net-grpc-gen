using System.ComponentModel;
using FluentAssertions;
using NetGrpcGen.ComponentModel;
using Xunit;

namespace NetGrpcGen.Tests
{
    public class DiscoveryServiceTests : BaseTests
    {
        [GrpcObject]
        public class TestObject
        {
            
        }

        [Fact]
        public void Can_discover_grpc_object()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObject));
            var result = discoveryService.DiscoverObjects();
            result.Count.Should().Be(1);
            result[0].Type.Should().Be(typeof(TestObject));
            result[0].ImplementedINotify.Should().BeFalse();
        }

        public class TestObjectWithoutAttribute
        {
            
        }
        
        [Fact]
        public void Can_ignore_objects_without_attribute()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObjectWithoutAttribute));
            var result = discoveryService.DiscoverObjects();
            result.Count.Should().Be(0);
        }

        [GrpcObject]
        public class TestObjectWithNotifyPropertyChanging : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate {  };
        }

        [Fact]
        public void Can_determine_if_implemented_notify_property()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObjectWithNotifyPropertyChanging));
            var result = discoveryService.DiscoverObjects();
            result.Count.Should().Be(1);
            result[0].ImplementedINotify.Should().BeTrue();
        }

        [GrpcObject]
        public class TestObjectWithProperty
        {
            [GrpcProperty]
            public string Prop { get; set; }
        }

        [Fact]
        public void Can_discover_property()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObjectWithProperty));
            var result = discoveryService.DiscoverObjects();
            result.Count.Should().Be(1);
            result[0].Properties.Should().HaveCount(1);
            var prop = result[0].Properties[0];
            prop.Name.Should().Be("Prop");
            prop.GrpcObject.Name.Should().Be("TestObjectWithProperty");
        }
    }
}
