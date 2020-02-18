using System.ComponentModel;
using System.Threading.Tasks;
using Custom.Types;
using NetGrpcGen.Adapters;
using NetGrpcGen.ComponentModel;
using Tests;

namespace NetGrpcGen.Tests.Objects
{
    [GrpcObject]
    public class Test2 : INotifyPropertyChanged
    {
        private string _propString;
        private TestMessageResponse _propComplex;

        [GrpcEvent]
        public virtual event GrpcObjectEventDelegate<string> TestEvent2 = delegate { };
        
        [GrpcEvent]
        public virtual event GrpcObjectEventDelegate<TestMessageResponse> TestEventComplex2 = delegate { };

        [GrpcEvent]
        public virtual event GrpcObjectEventDelegate TestEventNoData2 = delegate { };

        [GrpcMethod]
        public virtual Task<TestMessageResponse> TestMethod2(TestMessageRequest request)
        {
            return Task.FromResult(new TestMessageResponse
            {
                Value1 = request.Value1,
                Value2 = request.Value2
            });
        }
        
        [GrpcMethod]
        public virtual TestMessageResponse TestMethodSync2(TestMessageRequest request)
        {
            return new TestMessageResponse
            {
                Value1 = request.Value1,
                Value2 = request.Value2
            };
        }

        [GrpcMethod]
        public virtual void TestMethodWithNoResponse2(TestMessageRequest request)
        {
        }

        [GrpcMethod]
        public virtual void TestMethodNoRequest2()
        {
            
        }

        [GrpcProperty]
        public virtual string PropString2
        {
            get => _propString;
            set
            {
                _propString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PropString2"));
            }
        }

        [GrpcProperty]
        public virtual TestMessageResponse PropComplex2
        {
            get => _propComplex;
            set
            {
                _propComplex = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("PropComplex2"));
            }
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;
    }
    
    public class Test2Adapter : ObjectAdapter<Test2>
    {
        private readonly Test2 _instance;

        public Test2Adapter(Test2 instance)
        {
            _instance = instance;
            RegisterPropChangedType<Test2PropString2PropertyChanged>("PropString2");
            RegisterPropChangedType<Test2PropComplex2PropertyChanged>("PropComplex2");
            RegisterEventType<Test2TestEvent2Event>("TestEvent2");
            RegisterEventType<Test2TestEventComplex2Event>("TestEventComplex2");
            RegisterEventType<Test2TestEventNoData2Event>("TestEventNoData2");
        }
        
        public override Test2 Create()
        {
            if (_instance == null)
            {
                return new Test2();
            }
            return _instance;
        }
    }
}