namespace CompositeC1Contrib.ScheduledTasks.Cron
{
    internal class HoursCronSchedule : CronScheduleBase
    {
        public HoursCronSchedule(string expression)
        {
            Initialize(expression, 0, 23);
        }
    }
}
