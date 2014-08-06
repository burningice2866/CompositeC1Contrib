using Composite.Core.Application;
using Composite.Data.DynamicTypes;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    [ApplicationStartup]
    public sealed class StartupHandler
    {
        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IShopOrder));

            ECommerceWorker.Initialize();
        }
    }
}
