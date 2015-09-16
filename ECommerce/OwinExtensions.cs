using System.Web.Http;

using Composite.Data.DynamicTypes;

using CompositeC1Contrib.ECommerce.Data.Types;

using Owin;

namespace CompositeC1Contrib.ECommerce
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribECommerce(this IAppBuilder app, HttpConfiguration config)
        {
            config.Routes.MapHttpRoute("ECommerce", "ecommerce/{action}", new { controller = "ecommerce", action = "default" });

            DynamicTypeManager.EnsureCreateStore(typeof(IShopOrder));

            ECommerceWorker.Initialize();
        }
    }
}
