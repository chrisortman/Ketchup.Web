using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Web;

namespace Ketchup.Web.Mef.Web
{
    /// <summary>
    /// Utility class to simplify the code that we need to generate
    /// for webforms calls, it gives them a single method to call, instead of all this
    /// behavior.
    /// DO NOT USE THIS CLASS DIRECTLY
    /// </summary>
    public static class ControlUtility
    {
        public static ComposablePartCatalog InspectContainer(CompositionContainer container)
        {
            return null;
        }
        public static ComposablePartCatalog CreateCatalog(Type controlType)
        {
            var pdef = AttributedModelServices.CreatePartDefinition(controlType, null);
    
            var metadata = new Dictionary<string, object>();
            metadata[CompositionConstants.ExportTypeIdentityMetadataName] = AttributedModelServices.GetTypeIdentity(controlType);
            var exports = ReflectionModelServices.CreateExportDefinition(
                new LazyMemberInfo(controlType),
                AttributedModelServices.GetContractName(controlType),
                new Lazy<IDictionary<string, object>>(() => metadata), 
                null);

            var wrapped = ReflectionModelServices.CreatePartDefinition(
                new Lazy<Type>(() => controlType), 
                false, 
                new Lazy<IEnumerable<ImportDefinition>>(() => pdef.ImportDefinitions),
                new Lazy<IEnumerable<ExportDefinition>>(() => new [] { exports }), 
                new Lazy<IDictionary<string, object>>(() => metadata), 
                null);

            return new PartCatalog(wrapped);
        }

        public static CompositionContainer Container 
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.ApplicationInstance != null)
                {
                    var accessor = HttpContext.Current.ApplicationInstance as IScopedContainerAccessor;
                    if (accessor != null)
                    {
                        var container =  accessor.GetRequestLevelContainer();
                        return container;
                    }
                }
                return null;
            }
        }
    }

    public class PartCatalog : ComposablePartCatalog
    {
        private readonly ComposablePartDefinition _wrapped;

        public PartCatalog(ComposablePartDefinition wrapped)
        {
            _wrapped = wrapped;
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return new[] {_wrapped}.AsQueryable(); }
        }
    }
}
