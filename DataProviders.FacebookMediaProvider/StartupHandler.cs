using Composite.Core.Application;
using Composite.Data.DynamicTypes;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    [ApplicationStartup]
    public sealed class StartupHandler
    {
        public static object _lock = new object();

        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IFacebookAlbum));
        }
    }
}
