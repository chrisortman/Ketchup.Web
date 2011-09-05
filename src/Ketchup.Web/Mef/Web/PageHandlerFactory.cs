using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;

using Microsoft.ComponentModel.Composition.Extensions.Web;

namespace Microsoft.ComponentModel.Composition.WebExtensions
{
    public class RedirectToNotFoundHandler : IHttpHandler
    {
        #region Implementation of IHttpHandler

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests. </param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.RedirectToRoute("NotFoundUrl");
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable
        {
            get { return true; }
        }

        #endregion
    }
    public class MefPageHandlerFactory : PageHandlerFactory
    {
       // private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentDictionary<string, bool> _fileExistsCache =
            new ConcurrentDictionary<string, bool>();

        private bool CheckFileExists(string physicalPath)
        {
            bool fileExists = false;
            if(_fileExistsCache.ContainsKey(physicalPath))
            {
                fileExists = _fileExistsCache[physicalPath];
            }
            else
            {
                fileExists = File.Exists(physicalPath);
                _fileExistsCache.AddOrUpdate(physicalPath,fileExists,(k,v) => fileExists);
            }

            return fileExists;
        }

        public override IHttpHandler GetHandler(HttpContext context, string requestType, string virtualPath, string path)
        {
            
            if(!CheckFileExists(path))
            {
                return new RedirectToNotFoundHandler();
            }    

            var handler = base.GetHandler(context, requestType, virtualPath, path);
            
            if (handler != null)
            {
                var containerAccessor = context.ApplicationInstance as IScopedContainerAccessor;
                if(containerAccessor != null)
                {
                    // Todo: there may be an opportunity for optimization here, we dont want to reinspect the same type all the time
                    ComposablePart part = AttributedModelServices.CreatePart(handler);

                    // any imports?
                    if (part.ImportDefinitions.Any())
                    {
                        if (part.ExportDefinitions.Any())
                        {
                            // exports are not allowed
                            throw new Exception("Handlers cannot be exportable");
                        }

                        // then compose handler
                        // Todo: should happen in a per-request container
                        var batch = new CompositionBatch();
                        batch.AddPart(part);

                        var container = containerAccessor.GetRequestLevelContainer();

                        try
                        {
                            container.Compose(batch);
                        }
                        catch(Exception ex)
                        {
                           // log.Fatal("Unable to compose " + handler.GetType().FullName, ex);
                            if(context.Request.IsLocal)
                            {
                                context.Response.Write("<pre>" + ex.ToString() + "</pre>");
                            }
                            
                            throw;
                            
                        }
                    }
                }

            }

            return handler;
        }
    }
}
