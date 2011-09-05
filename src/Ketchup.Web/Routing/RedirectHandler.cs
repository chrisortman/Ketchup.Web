using System.Web;

namespace Ketchup.Web.Routing
{
    /// <summary>
    /// <para>Redirecting MVC handler</para>
    /// </summary>
    public class RedirectHandler : IHttpHandler
    {
        private string _newUrl;

        public RedirectHandler(string newUrl)
        {
            this._newUrl = newUrl;
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext httpContext)
        {
            if(_newUrl.StartsWith("~"))
            {
                _newUrl = VirtualPathUtility.ToAbsolute(_newUrl);
            }
            httpContext.Response.Status = "301 Moved Permanently";
            httpContext.Response.StatusCode = 301;
            httpContext.Response.AppendHeader("Location", _newUrl);
            return;
        }
    }
}