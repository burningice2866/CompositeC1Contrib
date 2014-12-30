using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

using Composite;

using CompositeC1Contrib.Rendering.Mvc.Functions;
using CompositeC1Contrib.Rendering.Mvc.Templates;

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

            AutoDiscoverTemplates();

            if (configuration.TemplateTypes != null)
            {
                foreach (var type in configuration.TemplateTypes)
                {
                    var template = new MvcPageTemplateDescriptor(type);

                    GlobalConfiguration.Current.Templates.Add(template);
                }
            }

            AutoDiscoverFunctions();

            var routes = RouteTable.Routes;
            var c1PageRoute = routes["c1 page route"];

            routes.Remove(c1PageRoute);

            if (configuration.RouteRegistrator != null)
            {
                configuration.RouteRegistrator(routes);
            }

            routes.Add("default mvc route", new DefaultCatchAllRoute());

            routes.Add(c1PageRoute);
        }

        private static void AutoDiscoverFunctions()
        {
            ForEachType(type =>
            {
                var typeName = type.Name;
                if (!typeName.EndsWith("Controller"))
                {
                    return;
                }

                var attribute = type.GetCustomAttributes<MvcFunctionAttribute>(false);
                if (attribute == null)
                {
                    return;
                }

                var controllerDescriptor = new ReflectedControllerDescriptor(type);
                var function = new MvcFunction(controllerDescriptor);

                GlobalConfiguration.Current.Functions.Add(function);
            });

            MvcFunctionsProvider.Reload();
        }

        private static void AutoDiscoverTemplates()
        {
            ForEachType(type =>
            {
                var attribute = type.GetCustomAttribute<MvcTemplateAttribute>(false);
                if (attribute == null)
                {
                    return;
                }

                var template = new MvcPageTemplateDescriptor(type);

                GlobalConfiguration.Current.Templates.Add(template);
            });
        }

        private static void ForEachType(Action<Type> action)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            try
                            {
                                action(type);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
