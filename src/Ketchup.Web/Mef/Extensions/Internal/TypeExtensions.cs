using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.ComponentModel.Composition.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Type StringType = typeof(string);
        private static readonly Type IEnumerableType = typeof(IEnumerable);
        private static readonly Type IEnumerableOfTType = typeof(IEnumerable<>);

        public static T New<T>(this Type type)
        {
            return (T) Activator.CreateInstance(type);
        }

        public static Type GetCollectionElementType(this Type type)
        {
            if (type == StringType || !IEnumerableType.IsAssignableFrom(type))
            {
                return null;
            }

            Type closedType;
            if (TryGetGenericInterfaceType(type, IEnumerableOfTType, out closedType))
            {
                return closedType.GetGenericArguments()[0];
            }

            return null;
        }

        internal static bool TryGetGenericInterfaceType(Type instanceType, Type targetOpenInterfaceType, out Type targetClosedInterfaceType)
        {
            // if instanceType is an interface, we must first check it directly
            if (instanceType.IsInterface &&
                instanceType.IsGenericType &&
                instanceType.GetGenericTypeDefinition() == targetOpenInterfaceType)
            {
                targetClosedInterfaceType = instanceType;
                return true;
            }

            try
            {
                // Purposefully not using FullName here because it results in a significantly
                //  more expensive implementation of GetInterface, this does mean that we're
                //  takign the chance that there aren't too many types which implement multiple
                //  interfaces by the same name...
                Type targetInterface = instanceType.GetInterface(targetOpenInterfaceType.Name, false);
                if (targetInterface != null &&
                    targetInterface.GetGenericTypeDefinition() == targetOpenInterfaceType)
                {
                    targetClosedInterfaceType = targetInterface;
                    return true;
                }
            }
            catch (AmbiguousMatchException)
            {
                // If there are multiple with the same name we should not pick any
            }

            targetClosedInterfaceType = null;
            return false;
        }
    }
}
