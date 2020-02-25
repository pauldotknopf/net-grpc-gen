using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests.TypeTests
{
    public class FloatTests : BaseTypeTests
    {
        [Fact]
        public async Task Can_call_with_float_param()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamFloat(3)).Returns(3);
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamFloatAsync(new TestTypesTestParamFloatMethodRequest
                {
                    ObjectId = objectId,
                    Value = 3
                });
                response.Value.Should().Be(3);
                o.Verify(x => x.TestParamFloat(3), Times.Once);
            });
        }
    }
}