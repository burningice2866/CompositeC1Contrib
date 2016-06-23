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
    }
}
