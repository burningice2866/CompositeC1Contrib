using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1RenderingPreviewUnsavedChangesAttribute : C1RenderingReasonAttribute
    {
        public C1RenderingPreviewUnsavedChangesAttribute() : base(RenderingReason.PreviewUnsavedChanges) { }
    }
}
