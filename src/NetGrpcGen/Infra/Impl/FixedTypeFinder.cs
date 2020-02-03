using System;
using System.Collections.Generic;
using System.Linq;

namespace NetGrpcGen.Infra.Impl
{
    public class FixedTypeFinder : ITypeFinder
    {
        private readonly List<Type> _types;

        public FixedTypeFinder(List<Type> types)
        {
            _types = types;
        }
        
        public IEnumerable<Type> GetTypes()
        {
            return _types.ToList();
        }
    }
}