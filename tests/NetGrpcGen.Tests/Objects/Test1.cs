using System;
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
    public class Test1 : INotifyPropertyChanged, IObjectCreated, IObjectReleased
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
        public virtual void TestMethodNoRequestOrResponse()
        {
        }

        [GrpcMethod]
        public virtual int TestMethodNoRequest()
        {
            return 1;
        }

        [GrpcMethod]
        public virtual void TestMethodNoResponse(int val)
        {
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

        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        public void ObjectCreated()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Factory.StartNew(() =>
            {
                return;
                var counter = 0;
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine("Invoking events!!");
                    try
                    {
                        counter++;
                        switch (counter)
                        {
                            case 1:
                                PropString = Guid.NewGuid().ToString();
                                break;
                            case 2:
                                PropString = null;
                                break;
                            case 3:
                                PropString = "";
                                break;
                            default:
                                counter = 0;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    Thread.Sleep(2000);
                }
            });
        }

        public void ObjectReleased()
        {
            _cancellationTokenSource.Cancel();
            _task.Wait();
        }
    }
}