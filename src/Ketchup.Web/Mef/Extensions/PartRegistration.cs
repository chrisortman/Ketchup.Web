using System;
using System.ComponentModel.Composition.Primitives;

namespace Ketchup.Web.Mef.Extensions
{
    public abstract class PartRegistration
    {
        public static NonGenericSinglePartRegistration Type(Type implementation)
        {
            return new NonGenericSinglePartRegistration(implementation);
        }

        public static SinglePartRegistration<TImplementation> Type<TImplementation>()
        {
            return new SinglePartRegistration<TImplementation>();
        }

        public static MultiplePartRegistration TypesDerivedFrom<TBase>() where TBase : class
        {
            return TypesDerivedFrom(typeof(TBase));
        }

        public static MultiplePartRegistration TypesDerivedFrom(Type baseType)
        {
            return new MultiplePartRegistration(type => !type.IsAbstract && !type.IsInterface && baseType.IsAssignableFrom(type));
        }

        public static MultiplePartRegistration TypesThatImplement<TInterface>()
        {
            return TypesThatImplement(typeof(TInterface));
        }

        public static MultiplePartRegistration TypesThatImplement(Type interfaceType)
        {
            return new MultiplePartRegistration(type => !type.IsAbstract && !type.IsInterface && interfaceType.IsAssignableFrom(type));
        }

        public static MultiplePartRegistration Types(Predicate<Type> selector)
        {
            return new MultiplePartRegistration(selector);
        }

        internal abstract bool IsMultipleRegistration { get; }

        internal abstract ComposablePartDefinition CreatePartFor(Type type, Convention convention);

        public abstract bool IsFor(Type implementation);
    }
}
