using System;
using System.Web;
using System.Web.Routing;

using Composite.Core.Routing;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1PageRouteConstraint : IRouteConstraint
    {
        private readonly Guid _pageId;

        public C1PageRouteConstraint(Guid pageId)
        {
            _pageId = pageId;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var path = httpContext.Request.Path;

            var pageUrlData = PageUrls.ParseUrl(path);
            if (pageUrlData == null)
            {
                return false;
            }

            return pageUrlData.PageId == _pageId;
        }
    }
}
