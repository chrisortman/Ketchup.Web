using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using Microsoft.ComponentModel.Composition.Extensions.Web.Internal;
using Microsoft.ComponentModel.Composition.WebExtensions.Internal;

namespace Microsoft.ComponentModel.Composition.Extensions.Web
{
    public class ScopedContainerManager
    {
       // private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private readonly ComposablePartCatalog _rootCatalog;
        private readonly CompositionContainer _container;
        private const string MefContainerKey = "mef-container";
        private const string ModeKey = "Mode";

        public ScopedContainerManager(ComposablePartCatalog rootCatalog)
        {
            if (rootCatalog == null) throw new ArgumentNullException("rootCatalog");

            _rootCatalog = rootCatalog;

            _container = new CompositionContainer(new FilteredCatalog(rootCatalog, def => GetAllWithinAppScope(def)), true);
        }

        public CompositionContainer ApplicationContainer
        {
            get { return _container; }
        }

        public void CreateRequestContainer(IDictionary dictionary, IEnumerable<ExportProvider> additionalExportProviders = null, Action<CompositionContainer> containerCreated = null)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");

            var catalog = new FilteredCatalog(_rootCatalog, def => GetAllWithinRequestScope(def));
            var exportProviders = new List<ExportProvider>()
            {
                ApplicationContainer
            };
            if(additionalExportProviders != null)
            {
                exportProviders.AddRange(additionalExportProviders);
            }

            var requestContainer = new CompositionContainer(catalog, false,exportProviders.ToArray());
            
            dictionary[MefContainerKey] = requestContainer;

            if(containerCreated != null)
            {
                containerCreated(requestContainer);
            }
        }

        public void EndRequestContainer(IDictionary dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");

            CompositionContainer requestContainer = (CompositionContainer)dictionary[MefContainerKey];

            if (requestContainer != null)
            {
                requestContainer.Dispose();
            }

            dictionary.Remove(MefContainerKey);
        }

        public CompositionContainer GetRequestContainer(IDictionary dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");

            CompositionContainer requestContainer = (CompositionContainer)dictionary[MefContainerKey];

            if (requestContainer == null)
            {
                throw new ApplicationException("No container available for this request");
            }

            return requestContainer;
        }

        public void Close()
        {
            if (_container != null)
                _container.Dispose();

            if (_rootCatalog != null)
                _rootCatalog.Dispose();
        }

        private static bool GetAllWithinAppScope(ComposablePartDefinition def)
        {
            bool isInScope =  def.ExportDefinitions.
                Any(ed =>
                    !ed.Metadata.ContainsKey(ModeKey) ||
                    (ed.Metadata.ContainsKey(ModeKey) && ((WebScopeMode)ed.Metadata[ModeKey]) == WebScopeMode.Application));
            return isInScope;
        }

        private static bool GetAllWithinRequestScope(ComposablePartDefinition def)
        {
            Contract.Requires(def != null);
            try
            {
                if(def == null || def.ExportDefinitions == null)
                {
                    return false;
                }
            }
            catch(NullReferenceException nre)
            {
                //if(log.IsWarnEnabled) log.Warn("Got an NRE trying to get export definitions",nre);
                return false;

            }

            bool isInScope =  def.ExportDefinitions.
                Any(ed => ed != null && ed.Metadata != null && 
                    (ed.Metadata.ContainsKey(ModeKey) && ((WebScopeMode)ed.Metadata[ModeKey]) == WebScopeMode.Request));
            return isInScope;
        }
    }
}
