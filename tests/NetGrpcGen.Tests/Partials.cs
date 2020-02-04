using NetGrpcGen.Adapters;

namespace Tests
{
    public partial class Test1CreateResponse : IObjectMessage
    {
    }

    public partial class Test1GetPropRequest : IObjectMessage, IPropertyMessage<Test1ObjectServiceProperty>
    {
        
    }

    public partial class Test1GetPropResponse : IObjectMessage, IPropertyMessage<Test1ObjectServiceProperty>
    {
        
    }

    public partial class Test1SetPropRequest : IObjectMessage, IPropertyMessage<Test1ObjectServiceProperty>
    {
        
    }
    
    public partial class Test1SetPropResponse : IObjectMessage, IPropertyMessage<Test1ObjectServiceProperty>
    {
        
    }

    public partial class Test1PropChanged : IObjectMessage, IPropertyMessage<Test1ObjectServiceProperty>
    {
        
    }

    public partial class Method1Request : IObjectMessage
    {
        
    }

    public partial class MethodWithReturnIntRequest : IObjectMessage
    {
        
    }

    public partial class MethodWithReturnIntARequest : IObjectMessage
    {
        
    }
}