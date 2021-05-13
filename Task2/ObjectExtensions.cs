using System;
using System.Reflection;

namespace Task2
{
    public static class ObjectExtensions
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public static void SetReadOnlyProperty<T>(this T obj, string propertyName, object newValue)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            
            if (propertyName is null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var type = typeof(T);
            do
            {
                var info = type.GetProperty(propertyName, Flags);
                if (info != null)
                {
                    if (info.CanWrite)
                    {
                        info.SetValue(obj, newValue);
                    }
                    else
                    {
                        (type.GetField($"<{propertyName}>k__BackingField", Flags) ?? throw new InvalidOperationException($"Can't find auto field for {propertyName} property."))
                            .SetValue(obj, newValue);
                    }
                    return;
                }

                type = type.BaseType;
            } 
            while (type != null);

            throw new InvalidOperationException($"Type {typeof(T).FullName} hasn't {propertyName} property.");
        }

        public static void SetReadOnlyField<T>(this T obj, string filedName, object newValue)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (filedName is null)
            {
                throw new ArgumentNullException(nameof(filedName));
            }

            var type = typeof(T);
            do
            {
                var info = type.GetField(filedName, Flags);
                if (info != null)
                {
                    info.SetValue(obj, newValue);
                    return;
                }

                type = type.BaseType;
            } 
            while (type != null);
            
            throw new InvalidOperationException($"Type {typeof(T).FullName} hasn't {filedName} field.");
        }
    }
}
