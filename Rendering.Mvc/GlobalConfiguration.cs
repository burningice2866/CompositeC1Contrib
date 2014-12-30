using System.Collections.Generic;

using CompositeC1Contrib.Rendering.Mvc.Functions;
using CompositeC1Contrib.Rendering.Mvc.Templates;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class GlobalConfiguration
    {
        public static GlobalConfiguration Current = new GlobalConfiguration();

        public IList<MvcPageTemplateDescriptor> Templates { get; private set; }
        public IList<MvcFunction> Functions { get; private set; }

        public GlobalConfiguration()
        {
            Templates = new List<MvcPageTemplateDescriptor>();
            Functions = new List<MvcFunction>();
        }
    }
}
