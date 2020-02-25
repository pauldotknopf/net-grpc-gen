using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests.TypeTests
{
    public class BoolTests : BaseTypeTests
    {
        [Fact]
        public async Task Can_call_with_bool_param()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamBool(true)).Returns(true);
            await WithWithObject(o.Object, async (client, instance, objectId) =>
            {
                var response = await client.InvokeTestParamBoolAsync(new TestTypesTestParamBoolMethodRequest
                {
                    ObjectId = objectId,
                    Value = true
                });
                response.Value.Should().Be(true);
                o.Verify(x => x.TestParamBool(true), Times.Once);
            });
        }
    }
}