using Composite.Core.Application;

namespace CompositeC1Contrib.ChangeHistory
{
    [ApplicationStartup]
    public sealed class StartupHandler
    {
        public static object _lock = new object();

        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            ChangeHistoryProcessor.Initialize();   
        }
    }
}
