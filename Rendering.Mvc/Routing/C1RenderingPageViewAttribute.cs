using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1RenderingPageViewAttribute : C1RenderingReasonAttribute
    {
        public C1RenderingPageViewAttribute() : base(RenderingReason.PageView) { }
    }
}
