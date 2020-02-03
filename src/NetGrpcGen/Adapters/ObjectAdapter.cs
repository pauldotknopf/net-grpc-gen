using System;

namespace NetGrpcGen.Adapters
{
    public abstract class ObjectAdapter<TObject,
        TGetPropRequest,
        TGetPropResponse>
    {
        public abstract TObject Create();

        public abstract TGetPropResponse GetProperty(TObject instance, TGetPropRequest request);
    }
}