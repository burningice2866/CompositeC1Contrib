using Composite.Core.Application;
using Composite.Data.DynamicTypes;
using CompositeC1Contrib.Security.Data.Types;

namespace DanskRetursystem.Udbyderportal.Security
{
    [ApplicationStartup]
    public sealed class StartupHandler
    {
        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IMembershipUser));
            DynamicTypeManager.EnsureCreateStore(typeof(IPagePermissions));
        }
    }
}
