namespace CompositeC1Contrib.ScheduledTasks.Cron
{
    internal class MinutesCronSchedule : CronScheduleBase
    {
        public MinutesCronSchedule(string expression)
        {
            Initialize(expression, 0, 59);
        }
    }
}
