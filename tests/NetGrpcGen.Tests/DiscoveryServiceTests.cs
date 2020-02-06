using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.Reflection;
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
        public class TestObjectWithMethod
        {
            [GrpcMethod]
            public DummyMessage1 TestMethod(DummyMessage2 request)
            {
                return new DummyMessage1();
            }
        }
        
        [Fact]
        public void Can_discover_method()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObjectWithMethod));
            var result = discoveryService.DiscoverObjects();
            result.Count.Should().Be(1);
            result[0].Methods.Should().HaveCount(1);
            result[0].Methods[0].Name.Should().Be("TestMethod");
            result[0].Methods[0].IsAsync.Should().BeFalse();
            result[0].Methods[0].RequestType.TypeName.Should().Be("DummyMessage2");
            result[0].Methods[0].ResponseType.TypeName.Should().Be("DummyMessage1");
        }

        [GrpcObject]
        public class TestObjectWithAsyncMethod
        {
            [GrpcMethod]
            public Task<DummyMessage1> TestMethod(DummyMessage2 request)
            {
                return Task.FromResult(new DummyMessage1());
            }
        }
        
        [Fact]
        public void Can_discover_async_method()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObjectWithAsyncMethod));
            var result = discoveryService.DiscoverObjects();
            result.Count.Should().Be(1);
            result[0].Methods.Should().HaveCount(1);
            result[0].Methods[0].Name.Should().Be("TestMethod");
            result[0].Methods[0].IsAsync.Should().BeTrue();
            result[0].Methods[0].RequestType.TypeName.Should().Be("DummyMessage2");
            result[0].Methods[0].ResponseType.TypeName.Should().Be("DummyMessage1");
        }

        [GrpcObject]
        public class TestObjectWithNoParameter
        {
            [GrpcMethod]
            public DummyMessage1 TestMethod()
            {
                return new DummyMessage1();
            }
        }
        
        [Fact]
        public void Can_not_discover_method_with_no_parameter()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObjectWithNoParameter));
            var ex = Assert.Throws<Exception>(() => discoveryService.DiscoverObjects());
            ex.Message.Should().Contain("All methods must have at least one parameter.");
        }

        [GrpcObject]
        public class TestObjectWithInvalidParameter
        {
            [GrpcMethod]
            public void TestMethod(DummyMessageInvalid request)
            {
                
            }

            public class DummyMessageInvalid : IMessage
            {
                public void MergeFrom(CodedInputStream input)
                {
                    throw new NotImplementedException();
                }

                public void WriteTo(CodedOutputStream output)
                {
                    throw new NotImplementedException();
                }

                public int CalculateSize()
                {
                    throw new NotImplementedException();
                }

                public MessageDescriptor Descriptor => throw new NotImplementedException();
            }
        }
        
        [Fact]
        public void All_methods_parameters_must_implement_object_message_interface()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObjectWithInvalidParameter));
            var ex = Assert.Throws<Exception>(() => discoveryService.DiscoverObjects());
            ex.Message.Should().Contain("Parameter must implement IObjectMessage.");
        }
        
        [GrpcObject]
        public class TestObjectWithNoResponse
        {
            [GrpcMethod]
            public void TestMethod(DummyMessage2 request)
            {
            }
        }
        
        [Fact]
        public void Can_discover_method_with_no_response()
        {
            var discoveryService = BuildDiscoveryService(typeof(TestObjectWithNoResponse));
            var result = discoveryService.DiscoverObjects();
            result.Count.Should().Be(1);
            result[0].Methods.Should().HaveCount(1);
            result[0].Methods[0].Name.Should().Be("TestMethod");
            result[0].Methods[0].RequestType.TypeName.Should().Be("DummyMessage2");
            result[0].Methods[0].ResponseType.TypeName.Should().Be("google.protobuf.Empty");
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
