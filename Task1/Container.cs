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
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var types = assembly.DefinedTypes.Where(IsValidType);
            foreach (var type in types)
            {
                var exportAttribute = type.GetCustomAttribute<ExportAttribute>(false);
                if (exportAttribute != null)
                {
                    AddType(type, exportAttribute.Contract ?? type);
                }
            }
        }

        public void AddType(Type type) => AddType(type, type);

        public void AddType(Type type, Type baseType)
        {
            IsValidType(type);
            IsValidBaseType(baseType, type);
            
            if (container.ContainsKey(type))
            {
                throw new ArgumentException("Type has already been added.", nameof(type));
            }

            container.Add(baseType, type);
        }

        public T Get<T>() => (T)GetInstance(typeof(T));
        
        private object GetInstance(Type sourceType)
        {
            var type = GetImplementationType(sourceType);

            var ctor = type.GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length)
                .First();
            var args = ctor.GetParameters()
                .Select(x => GetInstance(x.ParameterType)).ToArray();

            var obj = Activator.CreateInstance(type, args);
            InjectProperties(obj);

            return obj;
        }

        private static bool IsValidType(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.IsClass && !type.IsAbstract;
        }

        private static bool IsValidBaseType(Type baseType, Type type)
        {
            if (baseType is null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!baseType.IsAssignableFrom(type))
            {
                throw new ArgumentException($"{baseType} is not assignable from {type}");
            }

            return true;
        }

        private Type GetImplementationType(Type type)
        {
            if (container.TryGetValue(type, out var implementationType))
            {
                return implementationType;
            }

            return IsValidType(type) ? type : throw new ArgumentException($"Type {type} is not added.", nameof(type));
        }

        private void InjectProperties(object instance)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var propertyInfos= instance.GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<ImportAttribute>() != null);

            foreach (var propertyInfo in propertyInfos)
            {
                //If we do not check cross-references when adding types, we will most likely end up in an endless loop. 
                propertyInfo.SetValue(instance, GetInstance(propertyInfo.PropertyType));
            }
        }
    }
}