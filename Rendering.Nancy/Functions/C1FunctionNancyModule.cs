using System;
using System.Collections.Generic;
using System.Reflection;

using Composite.Functions;

using Nancy;
using Nancy.Routing;

namespace CompositeC1Contrib.Rendering.Nancy.Functions
{
    public abstract class C1FunctionNancyModule : NancyModule
    {
        public override IEnumerable<Route> Routes
        {
            get
            {
                var routes = base.Routes;
                var scopedRoutes = new List<Route>();

                var type = GetType();
                var attribute = type.GetCustomAttribute<NancyFunctionAttribute>();
                var ns = String.IsNullOrEmpty(attribute.Namespace) ? type.Namespace : attribute.Namespace;
                var name = String.IsNullOrEmpty(attribute.Name) ? type.Name : attribute.Name;
                var scope = ns + "." + name;

                foreach (var route in routes)
                {
                    var descriptor = new RouteDescription(route.Description.Name, route.Description.Method, "/" + scope + route.Description.Path, route.Description.Condition);
                    var newRoute = new Route(descriptor, route.Action);

                    scopedRoutes.Add(newRoute);
                }

                return scopedRoutes.AsReadOnly();
            }
        }

        protected C1FunctionNancyModule()
        {
            Before.AddItemToStartOfPipeline(BindFunctionParameters);
        }

        private Response BindFunctionParameters(NancyContext ctx)
        {
            var parameters = (ParameterList) ctx.Items["FunctionParameters"];

            foreach (var paramName in parameters.AllParameterNames)
            {
                var prop = GetType().GetProperty(paramName);
                if (prop == null)
                {
                    continue;
                }

                var value = parameters.GetParameter(paramName);

                prop.SetValue(this, value);
            }

            return null;
        }
    }
}
