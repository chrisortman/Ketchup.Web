using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace Microsoft.ComponentModel.Composition.Extensions
{
    public class AssemblyDiscoveryCatalog : ComposablePartCatalog
    {
        private readonly object _locker = new object();
        private IQueryable<ComposablePartDefinition> _parts;

        public AssemblyDiscoveryCatalog(params Assembly[] assemblies)
        {
            this.Assemblies = assemblies;
        }

        private IEnumerable<Assembly> Assemblies { get; set; }

        public override IQueryable<ComposablePartDefinition> Parts 
        {
            get
            {
                if (_parts == null)
                {
                    lock (_locker)
                    {
                        if (_parts == null)
                        {
                            var parts = InspectAssembliesAndBuildPartDefinitions();

                            System.Threading.Thread.MemoryBarrier();

                            _parts = parts.AsQueryable();
                        }
                    }
                }

                return _parts;
            }
        }

        private IEnumerable<ComposablePartDefinition> InspectAssembliesAndBuildPartDefinitions()
        {
            var parts = new List<ComposablePartDefinition>();

            foreach(var assembly in this.Assemblies)
            {
                var attributes = assembly.GetCustomAttributes(typeof(DiscoveryAttribute), true);

                if (attributes.Length == 0)
                {
                    //parts.AddRange(new AssemblyCatalog(assembly).Parts);
                }
                else
                {
                    foreach (DiscoveryAttribute discoveryAtt in attributes)
                    {
                        var discovery = discoveryAtt.DiscoveryMethod.New<IDiscovery>();
                        var discoveredParts = discovery.BuildPartDefinitions(assembly.GetTypes());

                        parts.AddRange(discoveredParts);
                    }
                }
            }

            return parts;
        }
    }
}
