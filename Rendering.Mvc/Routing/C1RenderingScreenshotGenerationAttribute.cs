using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1RenderingScreenshotGenerationAttribute : C1RenderingReasonAttribute
    {
        public C1RenderingScreenshotGenerationAttribute() : base(RenderingReason.ScreenshotGeneration) { }
    }
}
