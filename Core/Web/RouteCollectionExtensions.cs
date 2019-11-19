using System.Web;
using System.Web.Routing;

namespace CompositeC1Contrib.Web
{
    public static class RouteCollectionExtensions
    {
        public static void AddGenericHandler<T>(this RouteCollection routes, string route) where T : IHttpHandler, new()
        {
            routes.Add(new Route(route, new GenericRouteHandler<T>()));
        }

        public static void AddGenericHandler<T>(this RouteCollection routes, string route, RouteValueDictionary constraints) where T : IHttpHandler, new()
        {
            var r = new Route(route, new GenericRouteHandler<T>())
            {
                Constraints = constraints
            };

            routes.Add(r);
        }

        public static void AddServiceHandler<T>(this RouteCollection routes, string route) where T : class, IHttpHandler
        {
            routes.Add(new Route(route, new ServiceBasedRouteHandler<T>()));
        }

        public static void AddServiceHandler<T>(this RouteCollection routes, string route, RouteValueDictionary constraints) where T : class, IHttpHandler
        {
            var r = new Route(route, new ServiceBasedRouteHandler<T>())
            {
                Constraints = constraints
            };

            routes.Add(r);
        }
    }
}
