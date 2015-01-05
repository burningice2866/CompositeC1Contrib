using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1ActionAttribute : ActionMethodSelectorAttribute
    {
        private IEnumerable<IRouteConstraint> _constraints;

        public string Page { get; set; }
        public string PageTemplates { get; set; }
        public string PageType { get; set; }
        public string Suffix { get; set; }
        public RenderingReason Reason { get; set; }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            var route = controllerContext.RouteData.Route as Route;
            if (route == null)
            {
                throw new InvalidOperationException(String.Format("Route is not of type '{0}'", typeof(Route).FullName));
            }

            if (_constraints == null)
            {
                var routeAttribute = new C1RouteAttribute
                {
                    Page = Page,
                    PageTemplates = PageTemplates,
                    PageType = PageType,
                    Suffix = Suffix,
                    Reason = Reason
                };

                _constraints = routeAttribute.Constraints.Values.OfType<IRouteConstraint>();
            }

            var ctx = controllerContext.HttpContext;
            var values = controllerContext.RouteData.Values;

            return _constraints.All(c => c.Match(ctx, route, "action", values, RouteDirection.IncomingRequest));
        }
    }
}
