using System.Collections.Generic;
using System.Linq;

using Hangfire;
using Hangfire.Server;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class ScheduledTasksConfiguration
    {
        private readonly IList<IBackgroundProcess> _backgroundProcesses = new List<IBackgroundProcess>();

        public JobStorage JobStorage { get; set; }
        public BackgroundJobServerOptions ServerOptions { get; set; }

        public void AddBackgroundProcess(IBackgroundProcess backgroundProcess)
        {
            _backgroundProcesses.Add(backgroundProcess);
        }

        public IBackgroundProcess[] GetBackgroundProcesses()
        {
            return _backgroundProcesses.ToArray();
        }
    }
}
