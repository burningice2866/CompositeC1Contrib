using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

using Composite.Data;
using Composite.Data.DynamicTypes;

using CompositeC1Contrib.ECommerce.Configuration;
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

            Upgrade();
        }

        private static void Upgrade()
        {
            var defaultCurrency = ECommerceSection.GetSection().DefaultCurrency;

            using (var data = new DataConnection())
            {
                var update = new List<IShopOrder>();

                var orders = data.Get<IShopOrder>().Where(o => o.Currency == null || o.Currency.Length == 0).ToList();
                foreach (var order in orders)
                {
                    order.Currency = defaultCurrency.ToString();

                    update.Add(order);
                }

                data.Update<IShopOrder>(update);
            }
        }
    }
}
