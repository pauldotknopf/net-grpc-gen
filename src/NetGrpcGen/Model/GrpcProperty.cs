using System.Reflection;
using NetGrpcGen.ComponentModel;

namespace NetGrpcGen.Model
{
    public class GrpcProperty
    {
        public string Name { get; set; }
        
        public GrpcPropertyAttribute Attribute { get; set; }
        
        public GrpcObject GrpcObject { get; set; }
        
        public PropertyInfo Property { get; set; }
        
        public GrpcPropertyType PropertyType { get; set; }
    }
}