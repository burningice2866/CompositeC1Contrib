using System.Web.Routing;

using Composite.Data.DynamicTypes;

using CompositeC1Contrib.ECommerce.Data.Types;
using CompositeC1Contrib.ECommerce.Web;
using CompositeC1Contrib.ScheduledTasks;

using Owin;

namespace CompositeC1Contrib.ECommerce
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribECommerce(this IAppBuilder app, ScheduledTasksConfiguration scheduledTasksConfig)
        {
            RouteTable.Routes.Add(new Route("ecommerce/{*pathInfo}", new GenericRouteHandler<ECommerceHttpHandler>()));

            scheduledTasksConfig.AddBackgroundProcess(new ECommerceBackgroundProcess());

            DynamicTypeManager.EnsureCreateStore(typeof(IShopOrder));
            DynamicTypeManager.EnsureCreateStore(typeof(IShopOrderLog));
        }
    }
}
