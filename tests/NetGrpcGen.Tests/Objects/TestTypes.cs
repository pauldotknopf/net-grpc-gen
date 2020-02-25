using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using NetGrpcGen.ComponentModel;

namespace NetGrpcGen.Tests.Objects
{
    [GrpcObject]
    public class TestTypes
    {
        [GrpcMethod]
        public virtual double TestParamDouble(double val)
        {
            return val;
        }

        [GrpcMethod]
        public virtual float TestParamFloat(float val)
        {
            return val;
        }

        [GrpcMethod]
        public virtual int TestParamInt(int val)
        {
            return val;
        }

        [GrpcMethod]
        public virtual uint TestParamUInt(uint val)
        {
            return val;
        }

        [GrpcMethod]
        public virtual long TestParamLong(long val)
        {
            return val;
        }
        
        [GrpcMethod]
        public virtual ulong TestParamULong(ulong val)
        {
            return val;
        }

        [GrpcMethod]
        public virtual bool TestParamBool(bool val)
        {
            return val;
        }

        [GrpcMethod]
        public virtual string TestParamString(string val)
        {
            return val;
        }

        [GrpcMethod]
        public virtual byte TestParamByte(byte val)
        {
            return val;
        }
        
        [GrpcMethod]
        public virtual byte[] TestParamBytes(byte[] val)
        {
            return val;
        }
        
        [GrpcEvent]
        public virtual event GrpcObjectEventDelegate<string> TestEvent = delegate { };
    }
}