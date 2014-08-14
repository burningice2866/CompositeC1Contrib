using System;
using System.Reflection;

using CompositeC1Contrib.ScheduledTasks.Cron;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class ScheduledTask
    {
        private readonly CronSchedule _cronSchedule;

        public string Name { get; set; }
        public DateTime LastRun { get; private set; }
        public DateTime NextRun { get; private set; }

        public string CronExpression
        {
            get { return _cronSchedule.ToString(); }
        }

        public Type Type { get; set; }
        public string MethodName { get; set; }

        private MethodInfo _methodInfo;
        public MethodInfo MethodInfo
        {
            get { return _methodInfo ?? (_methodInfo = Type.GetMethod(MethodName)); }
        }

        public ScheduledTask(string cronExpression) : this(cronExpression, null, null) { }

        public ScheduledTask(string cronExpression, DateTime? lastRun, DateTime? nextRun)
        {
            _cronSchedule = CronSchedule.Parse(cronExpression);

            LastRun = lastRun.HasValue ? lastRun.Value : DateTime.MinValue;
            NextRun = nextRun.HasValue ? nextRun.Value : GetNextRun(DateTime.Now);
        }

        private DateTime GetNextRun(DateTime start)
        {
            DateTime nextRun;
            _cronSchedule.GetNext(start, out nextRun);

            NextRun = nextRun;

            return NextRun;
        }

        public bool ShouldBeExecuted(DateTime now)
        {
            return NextRun <= now;
        }

        public void Execute()
        {
            Log("Executing task '{0}'", Name);

            try
            {
                var methodInfo = MethodInfo;
                if (methodInfo == null)
                {
                    Log("Method '{0}' not found", MethodName);

                    return;
                }

                Log("Executing task '{0}' with method '{1}' on type '{2}'", Name, methodInfo.Name, methodInfo.DeclaringType.Name);

                methodInfo.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Log("Unhandled exception in task '{0}'", Name);
                Log(ex.ToString());
            }
            finally
            {
                LastRun = DateTime.Now;
                NextRun = GetNextRun(LastRun);

                TasksPersistence.UpdateTasksDocument(this);

                Log("Task '{0}' complete, next run will be at '{1}'", Name, NextRun);
            }
        }

        private void Log(string message, params object[] arg)
        {
            Logger.AppendLogMessage(this, message, arg);
        }
    }
}
