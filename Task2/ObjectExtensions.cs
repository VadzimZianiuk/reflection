using System;
using System.Reflection;

namespace Task2
{
    public static class ObjectExtensions
    {
        private const BindingFlags SearchFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public static void SetReadOnlyProperty(this object obj, string propertyName, object newValue)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            
            if (propertyName is null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var type = obj.GetType();
            do
            {
                var info = type.GetProperty(propertyName, SearchFlags);
                if (info != null)
                {
                    if (info.CanWrite)
                    {
                        info.SetValue(obj, newValue);
                    }
                    else
                    {
                        var fieldInfo = type.GetField($"<{propertyName}>k__BackingField", SearchFlags)
                            ?? throw new InvalidOperationException($"Can't find auto field for {propertyName} property.");
                        fieldInfo.SetValue(obj, newValue);
                    }

                    return;
                }

                type = type.BaseType;
            } 
            while (type != null);

            throw new InvalidOperationException($"Type {obj.GetType().FullName} hasn't {propertyName} property.");
        }
        
        public static void SetReadOnlyField(this object obj, string filedName, object newValue)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (filedName is null)
            {
                throw new ArgumentNullException(nameof(filedName));
            }

            var type = obj.GetType();
            do
            {
                var info = type.GetField(filedName, SearchFlags);
                if (info != null)
                {
                    info.SetValue(obj, newValue);
                    return;
                }

                type = type.BaseType;
            } 
            while (type != null);
            
            throw new InvalidOperationException($"Type {obj.GetType().FullName} hasn't {filedName} field.");
        }
    }
}
