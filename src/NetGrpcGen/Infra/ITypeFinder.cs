using System;
using System.Collections.Generic;
using NetGrpcGen.Model;

namespace NetGrpcGen.Infra
{
    public interface ITypeFinder
    {
        IEnumerable<Type> GetTypes();
    }
}