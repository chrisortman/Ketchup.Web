using System;
using System.Reflection;

namespace Microsoft.ComponentModel.Composition.Extensions
{
    public static class ReflectionExtensions
    {
        public static Type UnderlyingMemberType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return (member as PropertyInfo).PropertyType;
                case MemberTypes.Field:
                    return (member as FieldInfo).FieldType;
                case MemberTypes.TypeInfo:
                    return (member as Type);
                default:
                    throw new Exception("Unsupported member type " + member.MemberType);
            }
        }
    }
}
