using System.Web;
using System.Web.Routing;

namespace Ketchup.Web.Routing
{
    /// <summary>
    /// Redirect Route Handler
    /// </summary>
    public class RedirectRouteHandler : IRouteHandler
    {
        private readonly string newUrl;

        public RedirectRouteHandler(string newUrl)
        {
            this.newUrl = newUrl;
        }

        public string RedirectToUrl
        {
            get {
                return newUrl;
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new RedirectHandler(newUrl);
        }
    }
}