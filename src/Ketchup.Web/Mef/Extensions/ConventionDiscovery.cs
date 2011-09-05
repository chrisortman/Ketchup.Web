using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Microsoft.ComponentModel.Composition.Extensions
{
    public class ConventionDiscovery : IDiscovery
    {
        private readonly Convention _convention;
        private readonly List<PartRegistration> _registrations = new List<PartRegistration>();

        public ConventionDiscovery(params PartRegistration[] registrations)
            : this(new Convention(), registrations)
        {
        }

        public ConventionDiscovery(Convention customConvention, params PartRegistration[] registrations)
        {
            this._convention = customConvention;
            this._registrations.AddRange(registrations);
        }

        public void Add(PartRegistration registration)
        {
            this._registrations.Add(registration);
        }

        public void Update(Type implementation, Action<NonGenericSinglePartRegistration> stuff)
        {
            // ** *WON"T WORK ON MULTIPLE

            foreach(var r in _registrations)
            {
                if(r.IsFor(implementation) && r is NonGenericSinglePartRegistration)
                {
                    stuff((NonGenericSinglePartRegistration)r);
                }
            }
        }

        IEnumerable<ComposablePartDefinition> IDiscovery.BuildPartDefinitions(IEnumerable<Type> types)
        {
            var parts = new List<ComposablePartDefinition>();
            var multipleRegs = this._registrations.Where(reg => reg.IsMultipleRegistration);
            var singleRegs = this._registrations.Where(reg => !reg.IsMultipleRegistration);

            foreach (var reg in singleRegs)
            {
                var part = reg.CreatePartFor(null, this._convention);
                
                if (part == null) throw new Exception(); // "cant" happen

                parts.Add(part);
            }

            foreach (var type in types)
            {
                foreach (var reg in multipleRegs)
                {
                    var part = reg.CreatePartFor(type, this._convention);

                    if (part != null)
                    {
                        parts.Add(part);
                    }
                }
            }

            return parts;
        }
    }
}
