using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests.TypeTests
{
    public class DoubleTests : BaseTypeTests
    {
        [Fact]
        public async Task Can_call_with_double_param()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamDouble(3)).Returns(3);
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamDoubleAsync(new TestTypesTestParamDoubleMethodRequest
                {
                    ObjectId = objectId,
                    Value = 3
                });
                response.Value.Should().Be(3);
                o.Verify(x => x.TestParamDouble(3), Times.Once);
            });
        }
    }
}