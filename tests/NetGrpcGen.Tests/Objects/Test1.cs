using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Custom.Types;
using NetGrpcGen.Adapters;
using NetGrpcGen.ComponentModel;
using Tests;

#pragma warning disable 67
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable EventNeverInvoked.Global

namespace NetGrpcGen.Tests.Objects
{
    [GrpcObject]
    public class Test1 : INotifyPropertyChanged
    {
        private string _propString;
        private TestMessageResponse _propComplex;

        [GrpcEvent]
        public virtual event GrpcObjectEventDelegate<string> TestEvent = delegate { };
        
        [GrpcEvent]
        public virtual event GrpcObjectEventDelegate<TestMessageResponse> TestEventComplex = delegate { };

        [GrpcEvent]
        public virtual event GrpcObjectEventDelegate TestEventNoData = delegate { };

        [GrpcMethod]
        public virtual Task<TestMessageResponse> TestMethod(TestMessageRequest request)
        {
            return Task.FromResult(new TestMessageResponse
            {
                Value1 = request.Value1,
                Value2 = request.Value2,
                Value3 = request.Value3
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

        [GrpcMethod]
        public int TestMethodPrimitive(int val)
        {
            return val;
        }

        [GrpcMethod]
        public virtual void TestMethodNoRequest()
        {
            Thread.Sleep(3000);
        }

        [GrpcProperty]
        public virtual string PropString
        {
            get => _propString;
            set
            {
                _propString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PropString"));
            }
        }

        [GrpcProperty]
        public virtual TestMessageResponse PropComplex
        {
            get => _propComplex;
            set
            {
                _propComplex = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("PropComplex"));
            }
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;
    }
}