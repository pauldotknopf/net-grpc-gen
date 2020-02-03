using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        public string Prop
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Test1Adapter : ObjectAdapter<Test1,
        Test1GetPropRequest,
        Test1GetPropResponse,
        Test1SetPropRequest,
        Test1SetPropResponse>
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

        public override Test1GetPropResponse GetProperty(Test1 instance, Test1GetPropRequest request)
        {
            var result = new Test1GetPropResponse
            {
                ObjectId = request.ObjectId,
                Prop = request.Prop
            };
            
            switch (request.Prop)
            {
                case Test1ObjectServiceProperty.Prop:
                    result.Str = instance.Prop;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }

        public override Test1SetPropResponse SetProperty(Test1 instance, Test1SetPropRequest request)
        {
            var result = new Test1SetPropResponse
            {
                ObjectId = request.ObjectId,
                Prop = request.Prop
            };
            
            switch (request.Prop)
            {
                case Test1ObjectServiceProperty.Prop:
                    instance.Prop = request.Str;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
    }
}