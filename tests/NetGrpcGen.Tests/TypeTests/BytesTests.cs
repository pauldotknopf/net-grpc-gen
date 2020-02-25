using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf;
using Moq;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests.TypeTests
{
    public class BytesTests : BaseTypeTests
    {
        [Fact]
        public async Task Can_call_with_bytes_param()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamBytes(new byte[]{0x4, 0x5})).Returns(new byte[]{0x6, 0x7});
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamBytesAsync(new TestTypesTestParamBytesMethodRequest
                {
                    ObjectId = objectId,
                    Value = ByteString.CopyFrom(0x04, 0x5)
                });
                response.Value.ToByteArray().Should().Equal(0x6, 0x7);
                o.Verify(x => x.TestParamBytes(new byte[]{0x4, 0x5}), Times.Once);
            });
        }
        
        [Fact]
        public async Task Can_call_with_bytes_param_null()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamBytes(null)).Returns((byte[])null);
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamBytesAsync(new TestTypesTestParamBytesMethodRequest
                {
                    ObjectId = objectId,
                    Value = null
                });
                response.Value.Should().BeNull();
                o.Verify(x => x.TestParamBytes(null), Times.Once);
            });
        }
    }
}