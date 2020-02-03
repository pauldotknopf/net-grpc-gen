using System;
using System.Threading.Tasks;
using NetGrpcGen.Infra;

namespace NetGrpcGen.Adapters
{
    public abstract class ObjectAdapter<TObject,
        TGetPropResponse,
        TSetPropRequest,
        TPropChanged,
        TPropertyEnum>
    {
        public abstract TObject Create();

        public abstract TPropertyEnum ParsePropertyEnum(string propertyName);

        public abstract void PackValue(TObject instance, TPropChanged dest);

        public abstract void PackValue(TObject instance, TGetPropResponse dest);
        
        public abstract void UnpackValue(TObject instance, TSetPropRequest source);

        public abstract Task<object> InvokeMethod(TObject instance, object request);
    }
}