using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetGrpcGen.Infra
{
    public interface IAttributeFinder
    {
        List<Tuple<Type, T>> FindObjectsWithAttribute<T>() where T : class;

        List<Tuple<PropertyInfo, T>> FindPropertiesWithAttribute<T>(Type type) where T : class;
        
        List<Tuple<MethodInfo, T>> FindMethodsWithAttribute<T>(Type type) where T : class;
    }
}