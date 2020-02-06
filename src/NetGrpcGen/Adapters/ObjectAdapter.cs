using System;

namespace NetGrpcGen.Adapters
{
    public abstract class ObjectAdapter<TObject>
    {
        public abstract TObject Create();

        public abstract Type GetPropChangedType(string propName);
    }
}