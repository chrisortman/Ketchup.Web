using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Ketchup.Web.Routing
{
    public static class OurRouteExtensions
    {

        public static HttpMethodBasedRoute MapHttpRoute(this RouteCollection routes,string routeName, string url,object defaults)
        {
            var route = new HttpMethodBasedRoute(url, new RouteValueDictionary(defaults), null, new MvcRouteHandler());
            routes.Add(routeName,route);
            return route;
        }

        /// <summary>
        /// Use when you have an old webforms page that you have converted
        /// to MVC, but you want to preserve the URL incase it was bookmarked or referenced
        /// elsewhere
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="webformsPath"></param>
        /// <param name="mvcPath"></param>
        /// <returns></returns>
        public static Route RedirectLegacyUrl(this RouteCollection routes, string webformsPath, string mvcPath)
        {
            var routePattern = "{pageName}.aspx";
            if(webformsPath.IndexOf('/') > -1)
            {
                //only handles single directory
                routePattern = "{directory}/" + routePattern;
            }

            var route = new Route(routePattern, null,
                                  new RouteValueDictionary(
                                      new {pageName = new WebformsPageThatNoLongerExistsRouteConstraint(webformsPath)}),
                                  new RedirectRouteHandler(mvcPath));
            routes.Add(route);
            return route;
        }

        /// <summary>
        /// Use when you want to have a named route for a webforms page and / or MVC complains 
        /// about not finding a controller and the URL is for a webforms page.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="routeName"></param>
        /// <param name="pagePath"></param>
        /// <param name="actualPagePath"></param>
        /// <returns></returns>
        public static Route MapWebFormsPage(this RouteCollection routes,string routeName, string pagePath, string actualPagePath)
        {
            var route = new Route(pagePath, null, new RouteValueDictionary(new
                                                                               {
                                                                                   xxx = new WebformsPageThatNeedsToBeRoutedConstraint(actualPagePath,routeName)
                                                                               }), new PageRouteHandler(actualPagePath, false));
            
            if(String.IsNullOrWhiteSpace(routeName))
            {
                routes.Add(route);
            }
            else
            {
                
                routes.Add(routeName,route);
            }
            return route;
        }
        
        public static Route MapWebFormsPage(this RouteCollection routes, string pagePath, string actualPagePath)
        {
            return MapWebFormsPage(routes, null, pagePath, actualPagePath);
        }

        private class WebformsPageThatNeedsToBeRoutedConstraint : IRouteConstraint
        {
            private readonly string _actualPagePath;
            private readonly string _nameOfAssociatedRoute;

            public WebformsPageThatNeedsToBeRoutedConstraint(string actualPagePath, string nameOfAssociatedRoute)
            {
                _actualPagePath = actualPagePath;
                _nameOfAssociatedRoute = nameOfAssociatedRoute;
            }

            #region Implementation of IRouteConstraint

            /// <summary>
            /// Determines whether the URL parameter contains a valid value for this constraint.
            /// </summary>
            /// <returns>
            /// true if the URL parameter contains a valid value; otherwise, false.
            /// </returns>
            /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param><param name="route">The object that this constraint belongs to.</param><param name="parameterName">The name of the parameter that is being checked.</param><param name="values">An object that contains the parameters for the URL.</param><param name="routeDirection">An object that indicates whether the constraint check is being performed when an incoming request is being handled or when a URL is being generated.</param>
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                //should not participate in url generation
                if(routeDirection == RouteDirection.IncomingRequest)
                {
                    if(_actualPagePath.Equals(httpContext.Request.AppRelativeCurrentExecutionFilePath))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if(String.IsNullOrWhiteSpace(_nameOfAssociatedRoute))
                    {
                        //if not a named route we dont want to 
                        //participate in url generation
                        return false;
                    }
                    else
                    {
                        var requestRouteValues = httpContext.Request.RequestContext.RouteData.Values;

                        if (requestRouteValues.Count == 0 && values.Count == 0)
                        {
                            return true;
                        }

                        if(DictionaryValuesMatch("controller",values,requestRouteValues))
                        {
                            if(DictionaryValuesMatch("action",values,requestRouteValues))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    
                }
            }

            #endregion

            private bool DictionaryValuesMatch(string key, RouteValueDictionary routeValues, RouteValueDictionary requestValues)
            {
                if(routeValues.ContainsKey(key))
                {
                    if(requestValues.ContainsKey(key))
                    {
                        return routeValues[key] == requestValues[key];
                    }
                    else
                    {

                        return false;

                    }
                }
                else
                {
                    return false;
                }
            }
        }
        private class WebformsPageThatNoLongerExistsRouteConstraint : IRouteConstraint
        {
            private readonly string _from;

            public WebformsPageThatNoLongerExistsRouteConstraint(string @from)
            {
                _from = from;
            }

            #region Implementation of IRouteConstraint

            /// <summary>
            /// Determines whether the URL parameter contains a valid value for this constraint.
            /// </summary>
            /// <returns>
            /// true if the URL parameter contains a valid value; otherwise, false.
            /// </returns>
            /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param><param name="route">The object that this constraint belongs to.</param><param name="parameterName">The name of the parameter that is being checked.</param><param name="values">An object that contains the parameters for the URL.</param><param name="routeDirection">An object that indicates whether the constraint check is being performed when an incoming request is being handled or when a URL is being generated.</param>
            public bool Match(HttpContextBase httpContext, 
                              Route route, 
                              string parameterName, 
                              RouteValueDictionary values, 
                              RouteDirection routeDirection)
            {
                if(routeDirection == RouteDirection.IncomingRequest)
                {
                    var pageName = values["pageName"];
                    if(pageName == null)
                    {
                        return false;
                    }
                    else if(pageName is string)
                    {
                        if(values["directory"] != null)
                        {
                            var directory = values["directory"].ToString();
                            if(String.IsNullOrWhiteSpace(directory) == false)
                            {
                                pageName = directory + "/" + pageName;
                            }
                        }
                        return (pageName+".aspx").Equals(_from,StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        return false;
                    }
                    
                }
                else
                {
                    return false;
                }
            }

            #endregion
        }
    }
}