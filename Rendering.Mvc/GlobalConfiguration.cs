using System.Collections.Generic;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class GlobalConfiguration
    {
        public static GlobalConfiguration Current = new GlobalConfiguration();

        public IList<MvcPageTemplateDescriptor> Templates { get; private set; }

        public GlobalConfiguration()
        {
            Templates = new List<MvcPageTemplateDescriptor>();
        }
    }
}
