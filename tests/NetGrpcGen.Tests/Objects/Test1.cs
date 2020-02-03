using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NetGrpcGen.Adapters;
using NetGrpcGen.ComponentModel;
using Tests;

namespace NetGrpcGen.Tests.Objects
{
    [GrpcObject]
    public class Test1 : INotifyPropertyChanged
    {
        private string _prop;

        [GrpcProperty]
        public virtual string Prop
        {
            get => _prop;
            set
            {
                if (_prop == value)
                {
                    return;
                }
                _prop = value;
                OnPropertyChanged();
            }
        }

        [GrpcMethod]
        public virtual void Method1()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Test1Adapter : ObjectAdapter<Test1,
        Test1GetPropResponse,
        Test1SetPropRequest,
        Test1PropChanged,
        Test1ObjectServiceProperty>
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

        public override Test1ObjectServiceProperty ParsePropertyEnum(string propertyName)
        {
            switch (propertyName)
            {
                case "Prop":
                    return Test1ObjectServiceProperty.Prop;
                default:
                    throw new NotSupportedException();
            }
        }

        public override void PackValue(Test1 instance, Test1GetPropResponse dest)
        {
            switch (dest.Prop)
            {
                case Test1ObjectServiceProperty.Prop:
                    dest.Str = instance.Prop;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public override void PackValue(Test1 instance, Test1PropChanged dest)
        {
            switch (dest.Prop)
            {
                case Test1ObjectServiceProperty.Prop:
                    dest.Str = instance.Prop;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public override void UnpackValue(Test1 instance, Test1SetPropRequest source)
        {
            switch (source.Prop)
            {
                case Test1ObjectServiceProperty.Prop:
                    instance.Prop = source.Str;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public override Task<object> InvokeMethod(Test1 instance, object request)
        {
            if (request is Method1Request method1Request)
            {
                instance.Method1();
                return Task.FromResult<object>(new Method1Response());
            }
            
            throw new NotSupportedException();
        }
    }
}