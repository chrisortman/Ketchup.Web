using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.ComponentModel.Composition.Extensions.Web;

namespace CustomerPortal.Web.Lib
{
    public class MefControllerFactory : IControllerFactory
    {
        // private const string ControllerExportEntryName = "controllerExport";
        private readonly ScopedContainerManager _scopeManager;
        private readonly bool _allowExperimentalFeatures;

        public MefControllerFactory(ScopedContainerManager scopeManager)
        {
            this._scopeManager = scopeManager;
            _allowExperimentalFeatures =
                Convert.ToBoolean(WebConfigurationManager.AppSettings["EnableExperimentalFeatures"]);
        }

        private bool NotExperimentalOrExperimentalAllowed(Lazy<IController, IDictionary<string, object>> export)
        {
            if(export.Metadata.ContainsKey("IsExperimental"))
            {
                bool isExperimental = (bool) export.Metadata["IsExperimental"];
                if(isExperimental)
                {
                    return _allowExperimentalFeatures;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        public IController CreateController(System.Web.Routing.RequestContext requestContext, string controllerName)
        {
            

                var contextItems = requestContext.HttpContext.Items;
                var container = _scopeManager.GetRequestContainer(contextItems);

                var controllers = container.GetExports<IController, IDictionary<string, object>>();

                var controllerExport = FindController(controllers, controllerName);

                if(controllerExport == null)
                {
                    controllerExport = FindController(controllers, "Error");
                    requestContext.RouteData.Values["Controller"] = "Error";
                    requestContext.RouteData.Values["action"] = "NotFound";
                }

                var instance = controllerExport.Value;
                container.SatisfyImportsOnce(instance);

                return instance;
            
        }

        private Lazy<IController, IDictionary<string, object>> FindController(IEnumerable<Lazy<IController, IDictionary<string, object>>> controllers, string controllerName)
        {
            return controllers.
                Where(exp =>
                      exp.Metadata.ContainsKey(Constants.ControllerNameMetadataName)
                      &&
                      exp.Metadata[Constants.ControllerNameMetadataName].ToString().ToLowerInvariant().Equals(
                          controllerName.ToLowerInvariant())
                      && NotExperimentalOrExperimentalAllowed(exp)).
                FirstOrDefault();
        }

        public void ReleaseController(IController controller)
        {
            //do nothing because the request level container will be disposed.
        }

        private static IEnumerable<string> GetNamespaceFromRoute(RequestContext requestContext)
        {
            object routeNamespacesObj;

            if(requestContext != null &&
               requestContext.RouteData.DataTokens.TryGetValue("Namespaces", out routeNamespacesObj))
            {
                return routeNamespacesObj as IEnumerable<string>;
            }

            return null;
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return SessionStateBehavior.Default;
        }
    }
}