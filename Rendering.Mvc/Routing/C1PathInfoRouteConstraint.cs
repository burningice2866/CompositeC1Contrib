using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Routing;
using System.Web.Routing;

using Composite.Core.Routing;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1PathInfoRouteConstraint : IRouteConstraint
    {
        private readonly RouteInfo _routeInfo;

        public C1PathInfoRouteConstraint(String suffix)
        {
            var routeFactory = new DirectRouteFactoryContext(String.Empty, String.Empty, new List<ActionDescriptor>(), new DefaultInlineConstraintResolver(), true);
            var builder = routeFactory.CreateBuilder(suffix);
            var route = new Route(builder.Template, builder.Defaults, builder.Constraints, null);

            _routeInfo = new RouteInfo(route);
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var path = httpContext.Request.Path;

            var pageUrlData = PageUrls.ParseUrl(path);
            if (pageUrlData == null)
            {
                return false;
            }

            var routeData = _routeInfo.GetRouteData(new Uri("http://c1pathinfo" + pageUrlData.PathInfo));
            if (routeData == null)
            {
                return false;
            }

            foreach (var key in routeData.Values.Keys)
            {
                values.Add(key, routeData.Values[key]);
            }

            return true;
        }
    }
}
