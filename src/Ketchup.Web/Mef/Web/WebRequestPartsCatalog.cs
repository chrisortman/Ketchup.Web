using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace Ketchup.Web.Mef.Web
{
    public class RequestValues
    {
        public const string SITE_URL = "CurrentWebRequest.SiteUrl";
        public const string WEB_REQUEST = "CurrentWebRequest.RequestBase";
    }


    public class CurrentIisApplicationInfo
    {
        private readonly HttpContextBase _currentReqestContext;

        public CurrentIisApplicationInfo(HttpContextBase currentRequestContext)
        {
            Contract.Requires(currentRequestContext != null);
            Contract.Requires(currentRequestContext.Request != null);
            Contract.Requires(currentRequestContext.Request.Url != null);

            Contract.Ensures(String.IsNullOrWhiteSpace(Host) == false);
            Contract.Ensures(VirtualDirectory != null);
            Contract.Ensures(VirtualDirectory.StartsWith("/") == false);

            _currentReqestContext = currentRequestContext;
            Host = _currentReqestContext.Request.Url.Host;
            VirtualDirectory =
                _currentReqestContext.Request.ApplicationPath == "/"
                    ? ""
                    : _currentReqestContext.Request.ApplicationPath.TrimStart('/');

        }

        public string Host { get; set; }
        public string VirtualDirectory { get; set; }
    }

    public class WebRequestPartsCatalog : ExportProvider
    {
        private readonly HttpContextBase _currentReqestContext;
        private readonly List<WebRequestExportDefinition> _definitions;

        private class WebRequestExportDefinition : ExportDefinition
        {
            public WebRequestExportDefinition(string contractName, IDictionary<string, object> metadata, Func<object> createObject) : base(contractName, metadata)
            {
                CreateObject = createObject;
            }

            public Func<object> CreateObject { get; set; }
        }

        public WebRequestPartsCatalog(HttpContextBase currentReqestContext)
        {
            _currentReqestContext = currentReqestContext;
            _definitions = new List<WebRequestExportDefinition>();
            
            var appInfoDef =
                new WebRequestExportDefinition(AttributedModelServices.GetContractName(typeof (CurrentIisApplicationInfo)),
                                     new Dictionary<string, object>()
                                     {
                                         {
                                         CompositionConstants.ExportTypeIdentityMetadataName,
                                         AttributedModelServices.GetTypeIdentity(typeof (CurrentIisApplicationInfo))
                                         }
                                     },
                                     () => new CurrentIisApplicationInfo(_currentReqestContext));
            
            _definitions.Add(appInfoDef);

            var webSessionDef =
                new WebRequestExportDefinition(AttributedModelServices.GetContractName(typeof(HttpSessionStateBase)),
                                               new Dictionary<string, object>()
                                               {
                                                   {
                                                   CompositionConstants.ExportTypeIdentityMetadataName,
                                                   AttributedModelServices.GetTypeIdentity(typeof(HttpSessionStateBase))
                                                   }

                                               }, () => _currentReqestContext.Session);

            _definitions.Add(webSessionDef);
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            Contract.Requires(definition != null);
            Contract.Ensures(Contract.Result<IEnumerable<Export>>() != null);

            var constraint = definition.Constraint.Compile();
            var foundExports = from d in _definitions
                               where constraint(d)
                               select new Export(d, d.CreateObject);

            if (definition.Cardinality == ImportCardinality.ZeroOrMore)
            {
                return foundExports.ToList();
            }
            else if (foundExports.Count() == 1)
            {
                return foundExports.ToList();
            }
            else
            {
                return Enumerable.Empty<Export>();
            }
        }

        
    }
}