using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Task1.DoNotChange;

namespace Task1
{
    public class Container
    {
        private readonly Dictionary<Type, Type> container = new Dictionary<Type, Type>();

        public void AddAssembly(Assembly assembly)
        {
            foreach (var type in CheckNull(assembly).DefinedTypes)
            {
                if (!type.IsAbstract)
                {
                    AddType(type, type.GetCustomAttribute<ExportAttribute>(false)?.Contract ?? type);
                }
            }
        }

        public void AddType(Type type) => AddType(type, type);

        public void AddType(Type type, Type baseType)
        {
            if (CheckNull(type).IsAbstract)
            {
                throw new ArgumentException("Type is abstract", nameof(type));
            }

            if (!CheckNull(baseType).IsAssignableFrom(type))
            {
                throw new ArgumentException($"{baseType} is not assignable from {type}");
            }

            if (container.ContainsKey(type))
            {
                throw new ArgumentException("Type has already been added.", nameof(type));
            }

            container.Add(type, type);
            if (type != baseType)
            {
                container.Add(baseType, type);
            }
        }

        public T Get<T>()
        {
            return GetHelper(typeof(T));

            dynamic GetHelper(Type sourceType)
            {
                if (!container.TryGetValue(sourceType, out var type))
                {
                    throw new ArgumentException($"Type {sourceType} is not added.", nameof(sourceType));
                }

                var parameterInfos = Array.Empty<ParameterInfo>();
                _ = type.GetConstructors()
                    .Any(ci =>
                    {
                        parameterInfos = ci.GetParameters();
                        return parameterInfos.Any(pi => !container.ContainsKey(pi.ParameterType));
                    });

                var parameters = new object[parameterInfos.Length];
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    //If we do not check cross-references when adding types, we will most likely end up in an endless loop. 
                    parameters[i] = GetHelper(parameterInfos[i].ParameterType);
                }
                var obj = Activator.CreateInstance(type, parameters);

                foreach (var propertyInfo in type.GetProperties())
                {
                    if (propertyInfo.GetCustomAttribute<ImportAttribute>() != null)
                    {
                        //If we do not check cross-references when adding types, we will most likely end up in an endless loop. 
                        propertyInfo.SetValue(obj, GetHelper(container[propertyInfo.PropertyType]));
                    }
                }

                return obj;
            }
        }

        private static T CheckNull<T>(T obj) => obj ?? throw new ArgumentNullException(nameof(obj));
    }
}