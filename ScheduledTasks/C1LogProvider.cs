using Hangfire.Logging;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class C1LogProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            return new C1Logger(name);
        }
    }
}
