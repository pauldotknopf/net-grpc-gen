using System;
using System.Collections.Generic;

namespace NetGrpcGen.Infra.Impl
{
    public class TypeFinder : ITypeFinder
    {
        public IEnumerable<Type> GetTypes()
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in a.GetTypes())
                {
                    yield return type;
                }
            }
        }
    }
}