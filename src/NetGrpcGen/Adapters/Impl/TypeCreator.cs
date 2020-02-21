using System;

namespace NetGrpcGen.Adapters.Impl
{
    public class TypeCreator<TObject> : ITypeCreator<TObject>
    {
        public TObject Create()
        {
            return Activator.CreateInstance<TObject>();
        }
    }
}