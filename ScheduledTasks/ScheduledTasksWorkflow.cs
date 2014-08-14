using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Workflow.Activities;

using Composite.C1Console.Workflow.Activities;
using Composite.Core.Extensions;
using Composite.Core.Threading;

using CompositeC1Contrib.ScheduledTasks.Configuration;

namespace CompositeC1Contrib.ScheduledTasks
{
    public sealed partial class ScheduledTasksWorkflow : FormsWorkflow
    {
        public static readonly string RootPath = HostingEnvironment.MapPath("~/App_Data/ScheduledTasks");

        private readonly IList<ScheduledTask> _tasks = new List<ScheduledTask>();

        static ScheduledTasksWorkflow()
        {
            FileUtils.CreateDirectoryIfNotExists(RootPath);
        }

        public ScheduledTasksWorkflow()
        {
            InitializeComponent();

            var section = ScheduledTasksSection.GetSection();
            foreach (ScheduledTaskElement element in section.Tasks)
            {
                var task = TasksPersistence.ParseTask(element);

                _tasks.Add(task);

                Logger.AppendLogMessage("Task {0} is registered, cron value is '{1}'", task.Name, task.CronExpression);
            }
        }

        private int GetTimeoutDuration()
        {
            if (HostingEnvironment.ApplicationHost.ShutdownInitiated())
            {
                return 10;
            }

            var now = DateTime.Now;

            if (_tasks.Any(t => t.ShouldBeExecuted(now)))
            {
                return 0;
            }

            var timeoutDuration = _tasks.Min(t => (int)Math.Ceiling((t.NextRun - now).TotalSeconds));
            if (timeoutDuration < 1)
            {
                return 1;
            }

            return timeoutDuration;
        }

        private void OnInitializeTimeout(object sender, EventArgs e)
        {
            var timeoutDuration = GetTimeoutDuration();

            Logger.AppendLogMessage("Timeout duration set to {0} seconds", timeoutDuration);

            ((DelayActivity)sender).TimeoutDuration = TimeSpan.FromSeconds(timeoutDuration);
        }

        private void scavengeCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var tasksToExecute = _tasks.Where(t => t.ShouldBeExecuted(now)).ToList();

            using (ThreadDataManager.EnsureInitialize())
            {
                foreach (var task in tasksToExecute)
                {
                    if (HostingEnvironment.ApplicationHost.ShutdownInitiated())
                    {
                        return;
                    }

                    try
                    {
                        task.Execute();
                    }
                    catch { } // Silent catch - this thread should not kill the AppDomain
                }
            }

            Logger.AppendLogMessage("{0} tasks executed '{1}'", tasksToExecute.Count, String.Join(", ", tasksToExecute.Select(t => t.Name)));
        }
    }
}
