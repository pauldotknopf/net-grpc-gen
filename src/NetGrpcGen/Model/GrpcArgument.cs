using System;

namespace NetGrpcGen.Model
{
    public class GrpcArgument
    {
        public string Name { get; set; }
        
        public GrpcDataType DataType { get; set; }
        
        public Type Type { get; set; }
    }
}