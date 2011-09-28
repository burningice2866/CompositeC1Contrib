using Composite.Core.Application;
using Composite.Data.DynamicTypes;

using CompositeC1Contrib.Favorites.Data.Types;

namespace CompositeC1Contrib.Favorites
{
    [ApplicationStartup]
    public sealed class StartupHandler
    {
        public static object _lock = new object();

        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IFavoriteFunction));
        }
    }
}
