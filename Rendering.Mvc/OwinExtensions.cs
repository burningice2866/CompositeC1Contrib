using System;
using System.Web.Routing;

using Composite;

using Owin;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribMvc(this IAppBuilder app, Action<IBootstrapperConfiguration> configurationAction)
        {
            Verify.ArgumentNotNull(app, "app");
            Verify.ArgumentNotNull(configurationAction, "configurationAction");

            var configuration = new BootstrapperConfiguration();

            configurationAction(configuration);

            if (configuration.Templates != null)
            {
                foreach (var template in configuration.Templates)
                {
                    GlobalConfiguration.Current.Templates.Add(template);
                }
            }

            var routes = RouteTable.Routes;
            var c1PageRoute = routes["c1 page route"];

            routes.Remove(c1PageRoute);
            
            if (configuration.RouteRegistrator != null)
            {
                configuration.RouteRegistrator(routes);
            }

            routes.Add("default mvc route", new DefaultMvcPageRoute());

            routes.Add(c1PageRoute);
        }
    }
}
