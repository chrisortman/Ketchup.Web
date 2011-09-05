using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Threading;

namespace Microsoft.ComponentModel.Composition.WebExtensions
{
    public static class CompositionHost
    {
        // Field is internal only to assist in testing
        internal static CompositionContainer _container = null;
        private static object _lockObject = new object();

        public static void Initialize(CompositionContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            CompositionContainer globalContainer = null;
            bool alreadyCreated = TryGetOrCreateContainer(() => container, out globalContainer);

            if (alreadyCreated)
            {
                throw new InvalidOperationException("InvalidOperationException_GlobalContainerAlreadyInitialized");
            }
        }

        public static CompositionContainer Initialize(
            params ComposablePartCatalog[] catalogs)
        {
            AggregateCatalog aggregateCatalog = new AggregateCatalog(catalogs);
            CompositionContainer container = new CompositionContainer(aggregateCatalog, true);
            try
            {
                CompositionHost.Initialize(container);
            }
            catch
            {
                container.Dispose();

                // NOTE : this is important, as this prevents the disposal of the catalogs passed as input arguments
                aggregateCatalog.Catalogs.Clear();
                aggregateCatalog.Dispose();

                throw;
            }

            return container;
        }

        internal static bool TryGetOrCreateContainer(
            Func<CompositionContainer> createContainer, 
            out CompositionContainer globalContainer)
        {
            bool alreadyCreated = true;
            if (_container == null)
            {
                var container = createContainer.Invoke();
                lock (_lockObject)
                {
                    if (_container == null)
                    {
                        Thread.MemoryBarrier();
                        _container = container;
                        alreadyCreated = false;
                    }
                }
            }
            globalContainer = _container;
            return alreadyCreated;
        }
    }
}
