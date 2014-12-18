using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class BootstrapperConfiguration : IBootstrapperConfiguration
    {
        public IEnumerable<MvcPageTemplateDescriptor> Templates { get; private set; }
        public Action<RouteCollection> RouteRegistrator { get; private set; }

        public void UseTemplates(params MvcPageTemplateDescriptor[] templates)
        {
            Templates = templates;
        }

        public void RegisterRoutes(Action<RouteCollection> action)
        {
            RouteRegistrator = action;
        }
    }
}
