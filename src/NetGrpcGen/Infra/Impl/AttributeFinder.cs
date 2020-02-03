using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetGrpcGen.Infra.Impl
{
    public class AttributeFinder : IAttributeFinder
    {
        private readonly ITypeFinder _typeFinder;

        public AttributeFinder(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }
        
        public List<Tuple<Type, T>> FindObjectsWithAttribute<T>() where T : class
        {
            var result = new List<Tuple<Type, T>>();

            foreach (var type in _typeFinder.GetTypes())
            {
                var attribute = type.GetCustomAttributes(typeof(T), false);
                if (attribute.Length > 0)
                {
                    result.Add(new Tuple<Type, T>(type, attribute[0] as T));
                }
            }
            
            return result;
        }

        public List<Tuple<PropertyInfo, T>> FindPropertiesWithAttribute<T>(Type type) where T : class
        {
            var result = new List<Tuple<PropertyInfo, T>>();
            
            foreach (var prop in type.GetProperties())
            {
                var attribute = prop.GetCustomAttributes(typeof(T)).ToList();
                if (attribute.Count > 0)
                {
                    result.Add(new Tuple<PropertyInfo, T>(prop, attribute[0] as T));
                }
            }

            return result;
        }
    }
}