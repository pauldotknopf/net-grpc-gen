using System.Security.Cryptography.X509Certificates;
using NetGrpcGen.ComponentModel;

namespace NetGrpcGen.Tests.Objects
{
    [GrpcObject]
    public class TestTypes
    {
        [GrpcMethod]
        public virtual void TestParamDouble(double val)
        {
            
        }

        [GrpcMethod]
        public virtual void TestParamFloat(float val)
        {
            
        }

        [GrpcMethod]
        public virtual void TestParamInt(int val)
        {
            
        }

        [GrpcMethod]
        public virtual void TestParamUInt(uint val)
        {
            
        }

        [GrpcMethod]
        public virtual void TestParamLong(long val)
        {
            
        }
        
        [GrpcMethod]
        public virtual void TestParamULong(ulong val)
        {
                
        }

        [GrpcMethod]
        public virtual void TestParamBool(bool val)
        {
            
        }

        [GrpcMethod]
        public virtual void TestParamString(string val)
        {
            
        }

        [GrpcMethod]
        public virtual void TestParamByte(byte val)
        {
            
        }
        
        [GrpcMethod]
        public virtual void TestParamBytes(byte[] val)
        {
            
        }
    }
}