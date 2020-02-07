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

        public List<Tuple<MethodInfo, T>> FindMethodsWithAttribute<T>(Type type) where T : class
        {
            var result = new List<Tuple<MethodInfo, T>>();
            
            foreach (var method in type.GetMethods())
            {
                var attribute = method.GetCustomAttributes(typeof(T)).ToList();
                if (attribute.Count > 0)
                {
                    result.Add(new Tuple<MethodInfo, T>(method, attribute[0] as T));
                }
            }

            return result;
        }

        public List<Tuple<EventInfo, T>> FindEventsWithAttribute<T>(Type type) where T : class
        {
            var result = new List<Tuple<EventInfo, T>>();
            
            foreach (var e in type.GetEvents())
            {
                var attribute = e.GetCustomAttributes(typeof(T)).ToList();
                if (attribute.Count > 0)
                {
                    result.Add(new Tuple<EventInfo, T>(e, attribute[0] as T));
                }
            }

            return result;
        }
    }
}