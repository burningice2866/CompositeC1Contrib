using System.Web;
using System.Web.Routing;

using Composite.Core.Routing;
using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1RenderingReasonConstraint : IRouteConstraint
    {
        private readonly RenderingReason _reason;

        public C1RenderingReasonConstraint(RenderingReason reason)
        {
            _reason = reason;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (PageRenderer.RenderingReason == RenderingReason.Undefined)
            {
                PageRenderer.RenderingReason = new UrlSpace(httpContext).ForceRelativeUrls ? RenderingReason.C1ConsoleBrowserPageView : RenderingReason.PageView;
            }

            return _reason == RenderingReason.Undefined || _reason == PageRenderer.RenderingReason;
        }
    }
}
