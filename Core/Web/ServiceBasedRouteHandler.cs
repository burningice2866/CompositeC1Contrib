using System.Web;
using System.Web.Routing;

using Composite.Core;

using Microsoft.Extensions.DependencyInjection;

namespace CompositeC1Contrib.Web
{
    public class ServiceBasedRouteHandler<T> : IRouteHandler where T : class, IHttpHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return ServiceLocator.GetService<T>() ?? ActivatorUtilities.CreateInstance<T>(ServiceLocator.ServiceProvider);
        }
    }
}
