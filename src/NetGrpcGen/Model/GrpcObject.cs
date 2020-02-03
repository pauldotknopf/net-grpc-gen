using System;
using System.Collections.Generic;
using NetGrpcGen.ComponentModel;

namespace NetGrpcGen.Model
{
    public class GrpcObject
    {
        public GrpcObject()
        {
            Properties = new List<GrpcProperty>();
            Methods = new List<GrpcMethod>();
        }
        
        public GrpcObjectAttribute Attribute { get; set; }
        
        public Type Type { get; set; }

        public bool ImplementedINotify { get; set; }
        
        public string Name { get; set; }
        
        public List<GrpcProperty> Properties { get; set; }
        
        public List<GrpcMethod> Methods { get; set; }
    }
}