using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using NetGrpcGen.Adapters;

namespace NetGrpcGen.Tests
{
    public class DummyMessage1 : IMessage
    {
        public void MergeFrom(CodedInputStream input)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(CodedOutputStream output)
        {
            throw new NotImplementedException();
        }

        public int CalculateSize()
        {
            throw new NotImplementedException();
        }

        public MessageDescriptor Descriptor => throw new NotImplementedException();
    }
    
    public class DummyMessage2 : IMessage
    {
        public void MergeFrom(CodedInputStream input)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(CodedOutputStream output)
        {
            throw new NotImplementedException();
        }

        public int CalculateSize()
        {
            throw new NotImplementedException();
        }

        public MessageDescriptor Descriptor => throw new NotImplementedException();
    }
}