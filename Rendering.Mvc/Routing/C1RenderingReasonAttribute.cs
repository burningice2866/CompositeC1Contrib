using System.Reflection;
using System.Web.Mvc;

using Composite.Core.Routing;
using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1RenderingReasonAttribute : ActionMethodSelectorAttribute
    {
        private readonly RenderingReason _renderingReason;

        public C1RenderingReasonAttribute(RenderingReason renderingReason)
        {
            _renderingReason = renderingReason;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            if (PageRenderer.RenderingReason == RenderingReason.Undefined)
            {
                PageRenderer.RenderingReason = new UrlSpace(controllerContext.HttpContext).ForceRelativeUrls ? RenderingReason.C1ConsoleBrowserPageView : RenderingReason.PageView;
            }

            return _renderingReason == RenderingReason.Undefined || _renderingReason == PageRenderer.RenderingReason;
        }
    }
}
