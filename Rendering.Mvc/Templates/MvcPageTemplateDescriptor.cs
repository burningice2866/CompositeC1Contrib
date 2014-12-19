using Composite.Core.PageTemplates;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class MvcPageTemplateDescriptor : PageTemplateDescriptor
    {
        public string ViewName { get; private set; }

        public MvcPageTemplateDescriptor(string viewName)
        {
            ViewName = viewName;
        }
    }
}
