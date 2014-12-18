using System;
using System.Web.Routing;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public interface IBootstrapperConfiguration
    {
        void UseTemplates(params MvcPageTemplateDescriptor[] templates);
        void RegisterRoutes(Action<RouteCollection> action);
    }
}
