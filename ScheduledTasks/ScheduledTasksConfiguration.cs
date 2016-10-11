using Hangfire;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class ScheduledTasksConfiguration
    {
        public JobStorage JobStorage { get; set; }
        public BackgroundJobServerOptions ServerOptions { get; set; }
    }
}
