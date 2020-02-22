using System.Threading.Tasks;
using Moq;
using NetGrpcGen.Tests.Objects;
using Tests;
using Xunit;

namespace NetGrpcGen.Tests.TypeTests
{
    public class IntTests : BaseTypeTests
    {
        [Fact]
        public async Task Can_call_with_int_param()
        {
            var o = new Mock<TestTypes>();
            o.Setup(x => x.TestParamInt(3));
            await WithWithObject(o.Object, async (client, stream, instance, objectId) =>
            {
                await client.InvokeTestParamIntAsync(new TestTypesTestParamIntMethodRequest
                {
                    ObjectId = objectId,
                    Value = 3
                });
                o.Verify(x => x.TestParamInt(3), Times.Once);
            });
        }
    }
}