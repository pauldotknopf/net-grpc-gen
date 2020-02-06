using System.Threading.Tasks;
using NetGrpcGen.Adapters;
using NetGrpcGen.ComponentModel;
using Tests;

namespace NetGrpcGen.Tests.Objects
{
    [GrpcObject]
    public class Test1
    {
        [GrpcMethod]
        public virtual Task<TestMessageResponse> TestMethod(TestMessageRequest request)
        {
            return Task.FromResult(new TestMessageResponse
            {
                Value1 = request.Value1,
                Value2 = request.Value2
            });
        }
        
        [GrpcMethod]
        public virtual TestMessageResponse TestMethodSync(TestMessageRequest request)
        {
            return new TestMessageResponse
            {
                Value1 = request.Value1,
                Value2 = request.Value2
            };
        }

        [GrpcMethod]
        public virtual void TestMethodWithNoResponse(TestMessageRequest request)
        {
            
        }
    }

    public class Test1Adapter : ObjectAdapter<Test1>
    {
        private readonly Test1 _instance;

        public Test1Adapter(Test1 instance)
        {
            _instance = instance;
        }
        
        public override Test1 Create()
        {
            return _instance;
        }
    }
}