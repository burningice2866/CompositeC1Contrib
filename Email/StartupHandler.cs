using Composite.Core.Application;
using Composite.Data.DynamicTypes;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    [ApplicationStartup]
    public sealed class StartupHandler
    {
        public static object _lock = new object();

        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IEmailQueue));
            DynamicTypeManager.EnsureCreateStore(typeof(IEmailMessage));
        }
    }
}
