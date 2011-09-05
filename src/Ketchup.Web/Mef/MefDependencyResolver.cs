using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ketchup.Web.Mef.Web;

namespace Ketchup.Web.Mef
{
    public class MefDependencyResolver : IDependencyResolver
    {
        //private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private readonly ScopedContainerManager _containerManager;
        
        public MefDependencyResolver(ScopedContainerManager containerManager)
        {
            _containerManager = containerManager;
        }

        public object GetService(Type serviceType)
        {
            Contract.Requires(serviceType != null);

            //want to just exit if this request is afor a view page
            //we cant handle those anyway...yet
            if(typeof(WebViewPage).IsAssignableFrom(serviceType))
            {
                return null;
            }

            string name = AttributedModelServices.GetContractName(serviceType);

            try
            {
                var container = _containerManager.GetRequestContainer(HttpContext.Current.Items);
                var exports = container.GetExports<object>(name);
                if(exports.Count() == 1)
                {
                    
                    return exports.First();
                }

                //if(log.IsErrorEnabled) log.Error("Could not resolve dependency for " + name);
                return null;
            }
            catch(Exception ex)
            {
                //log.Error("Error resolving dependency", ex);
                return null;
            }

        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            Contract.Requires(serviceType != null);

            var name = AttributedModelServices.GetContractName(serviceType);
            try
            {
                var container = _containerManager.GetRequestContainer(HttpContext.Current.Items);
                return container.GetExportedValues<object>(name);
            }
            catch(Exception ex)
            {
                //log.Error("Error resolving dependency", ex);
                return null;
            }
        }
    }

    public class MefFilterAttributeProvider : FilterAttributeFilterProvider
    {
        private ScopedContainerManager _scopeManager;

        public MefFilterAttributeProvider(ScopedContainerManager scopeManager)
        {
            _scopeManager = scopeManager;
        }

        protected override IEnumerable<FilterAttribute> GetControllerAttributes(
            ControllerContext controllerContext,
            ActionDescriptor actionDescriptor)
        {
            var attributes = base.GetControllerAttributes(controllerContext,
                                                          actionDescriptor);
            var requestContainer = _scopeManager.GetRequestContainer(controllerContext.HttpContext.Items);
            var batch = new CompositionBatch();
            foreach(var attribute in attributes)
            {
                batch.AddPart(attribute);
            }
            requestContainer.Compose(batch);
            return attributes;
        }

        protected override IEnumerable<FilterAttribute> GetActionAttributes(
            ControllerContext controllerContext,
            ActionDescriptor actionDescriptor)
        {
            var attributes = base.GetActionAttributes(controllerContext,
                                                      actionDescriptor);
            var requestContainer = _scopeManager.GetRequestContainer(controllerContext.HttpContext.Items);
            var batch = new CompositionBatch();
            foreach(var attribute in attributes)
            {
                batch.AddPart(attribute);
            }
            requestContainer.Compose(batch);
            return attributes;
        }

        public static void Register(ScopedContainerManager containerManager)
        {
            var oldProvider = FilterProviders.Providers.Single(
                f => f is FilterAttributeFilterProvider
                );
            FilterProviders.Providers.Remove(oldProvider);

            var provider = new MefFilterAttributeProvider(containerManager);
            FilterProviders.Providers.Add(provider);
        }
    }
}