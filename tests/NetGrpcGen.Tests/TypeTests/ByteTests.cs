using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests.TypeTests
{
    public class ByteTests : BaseTypeTests
    {
        [Fact]
        public async Task Can_call_with_byte_param()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamByte(0x3)).Returns(0x4);
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamByteAsync(new TestTypesTestParamByteMethodRequest
                {
                    ObjectId = objectId,
                    Value = 0x03
                });
                response.Value.Should().Be(0x4);
                o.Verify(x => x.TestParamByte(0x3), Times.Once);
            });
        }
    }
}