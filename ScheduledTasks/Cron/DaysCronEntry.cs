namespace CompositeC1Contrib.ScheduledTasks.Cron
{
    internal class DaysCronSchedule : CronScheduleBase
    {
        public DaysCronSchedule(string expression)
        {
            Initialize(expression, 1, 31);
        }
    }
}
