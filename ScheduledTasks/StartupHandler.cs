using Composite.Core.Application;
using Composite.Data.DynamicTypes;

using Hangfire.CompositeC1.Types;

namespace CompositeC1Contrib.ScheduledTasks
{
    [ApplicationStartup]
    public class StartupHandler
    {
        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(ICounter));
            DynamicTypeManager.EnsureCreateStore(typeof(IHash));
            DynamicTypeManager.EnsureCreateStore(typeof(IJob));
            DynamicTypeManager.EnsureCreateStore(typeof(IJobParameter));
            DynamicTypeManager.EnsureCreateStore(typeof(IJobQueue));
            DynamicTypeManager.EnsureCreateStore(typeof(IList));
            DynamicTypeManager.EnsureCreateStore(typeof(IServer));
            DynamicTypeManager.EnsureCreateStore(typeof(ISet));
            DynamicTypeManager.EnsureCreateStore(typeof(IState));
        }
    }
}
