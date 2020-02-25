using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests.TypeTests
{
    public class StringTests : BaseTypeTests
    {
        [Fact]
        public async Task Can_call_with_string_param_null()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamString(null)).Returns((string)null);
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamStringAsync(new TestTypesTestParamStringMethodRequest()
                {
                    ObjectId = objectId,
                    Value = null
                });
                response.Value.Should().BeNull();
                o.Verify(x => x.TestParamString(null), Times.Once);
            });
        }
        
        [Fact]
        public async Task Can_call_with_string_param_empty()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamString("")).Returns("");
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamStringAsync(new TestTypesTestParamStringMethodRequest()
                {
                    ObjectId = objectId,
                    Value = ""
                });
                response.Value.Should().Be("");
                o.Verify(x => x.TestParamString(""), Times.Once);
            });
        }
        
        [Fact]
        public async Task Can_call_with_string_param_unicode()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamString("汉字")).Returns("汉字");
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamStringAsync(new TestTypesTestParamStringMethodRequest()
                {
                    ObjectId = objectId,
                    Value = "汉字"
                });
                response.Value.Should().Be("汉字");
                o.Verify(x => x.TestParamString("汉字"), Times.Once);
            });
        }
    }
}