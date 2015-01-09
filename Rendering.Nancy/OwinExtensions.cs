using System;
using System.Reflection;

using Composite;

using CompositeC1Contrib.Rendering.Nancy.Functions;

using Owin;

namespace CompositeC1Contrib.Rendering.Nancy
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribNancy(this IAppBuilder app)
        {
            Verify.ArgumentNotNull(app, "app");

            AutoDiscoverFunctions();
        }

        private static void AutoDiscoverFunctions()
        {
            ForEachType(type =>
            {
                var typeName = type.Name;
                if (!typeName.EndsWith("Module") || !typeof(C1FunctionNancyModule).IsAssignableFrom(type))
                {
                    return;
                }

                var attribute = type.GetCustomAttribute<NancyFunctionAttribute>(false);
                if (attribute == null)
                {
                    return;
                }

                var function = new NancyFunction(type);

                GlobalConfiguration.Current.Functions.Add(function);
            });

            NancyFunctionsProvider.Reload();
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
