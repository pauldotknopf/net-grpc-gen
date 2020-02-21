using System;

namespace NetGrpcGen.Adapters
{
    public interface ITypeCreator<TObject>
    {
        TObject Create();
    }
}