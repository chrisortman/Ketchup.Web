using System.Web;
using System.Web.Routing;

namespace Ketchup.Web.Routing
{
    public class HttpMethodBasedRoute : Route
    {
        public HttpMethodBasedRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler) : base(url, defaults, constraints, routeHandler)
        {
        }

        public override RouteData GetRouteData(HttpContextBase httpContext) {
            var routeData =  base.GetRouteData(httpContext);

            if(routeData != null && 
               routeData.Values.ContainsKey(httpContext.Request.HttpMethod.ToLower()))
            {
                routeData.Values["action"] = routeData.Values[httpContext.Request.HttpMethod.ToLower()];
            }
            return routeData;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values) {
            var vp =  base.GetVirtualPath(requestContext, values);
            return vp;
        }
    }
}