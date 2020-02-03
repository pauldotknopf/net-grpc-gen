using System;

namespace NetGrpcGen.Adapters
{
    public abstract class ObjectAdapter<TObject,
        TGetPropRequest,
        TGetPropResponse,
        TSetPropRequest,
        TSetPropResponse>
    {
        public abstract TObject Create();

        public abstract TGetPropResponse GetProperty(TObject instance, TGetPropRequest request);
        
        public abstract TSetPropResponse SetProperty(TObject instance, TSetPropRequest request);
    }
}