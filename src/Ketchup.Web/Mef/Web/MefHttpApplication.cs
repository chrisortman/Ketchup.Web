using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using Ketchup.Web.Mef.Extensions;

namespace Ketchup.Web.Mef.Web
{
    /// <summary>
    /// Uses the application events to signal to the
    /// <see cref="ScopedContainerManager"/> the need to  
    /// create/dispose request containers.
    /// </summary>
    public class MefHttpApplication : HttpApplication, IScopedContainerAccessor
    {
        private static ScopedContainerManager _containerManager;

        public MefHttpApplication()
        {
            this.BeginRequest += new EventHandler(OnBeginRequest);
            this.EndRequest += new EventHandler(OnEndRequest);
        }

        protected ScopedContainerManager ScopeManager
        {
            get { return _containerManager; }
        }

        CompositionContainer IScopedContainerAccessor.GetRequestLevelContainer()
        {
            var items = HttpContext.Current.Items;
            return _containerManager.GetRequestContainer(items);
        }

        // Uncomment if you really need to access the container in a "static" way 
        // which we really don't want to encourage people to do.
        //public static CompositionContainer ApplicationContainer
        //{
        //    get { return _containerManager.ApplicationContainer; }
        //}

        protected virtual void Application_Start()
        {
            InitializeContainerManager();
        }

        protected void InitializeContainerManager()
        {
            if (_containerManager == null)
            {
                var catalog = CreateRootCatalog();

                _containerManager = new ScopedContainerManager(catalog);
            }
        }

        protected virtual void Application_End()
        {
            if (_containerManager != null)
            {
                _containerManager.Close();
            }
        }

        //protected virtual ComposablePartCatalog CreateRootCatalog()
        //{
        //    return new DirectoryCatalog(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"));
        //}

        protected virtual ComposablePartCatalog CreateRootCatalog()
        {
            var agg = new AggregateCatalog();

            foreach(Assembly assembly in BuildManager.GetReferencedAssemblies())
            {
                agg.Catalogs.Add(new AssemblyDiscoveryCatalog(assembly));
                agg.Catalogs.Add(new AssemblyCatalog(assembly));
            }

            //agg.Catalogs.Add(new DirectoryCatalog(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")));

            return agg;
        }

        protected virtual void OnBeginRequest(object sender, EventArgs e)
        {
            var httpContext = HttpContext.Current;
            _containerManager.CreateRequestContainer(httpContext.Items,
                                                     additionalExportProviders: new ExportProvider[]
                                                     {
                                                         new WebRequestPartsCatalog(new HttpContextWrapper(httpContext)) ,
                                                         new ConfigFileExportProvider()
                                                     },
                                                     containerCreated:
                                                         container => RequestContainerCreated(httpContext, container));

        }

        protected virtual void RequestContainerCreated(HttpContext context, CompositionContainer container) {}

        private void OnEndRequest(object sender, EventArgs e)
        {
            _containerManager.EndRequestContainer(HttpContext.Current.Items);
        }
    }
}
