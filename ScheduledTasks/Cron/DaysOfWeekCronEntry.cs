namespace CompositeC1Contrib.ScheduledTasks.Cron
{
    internal class DaysOfWeekCronSchedule : CronScheduleBase
    {
        public DaysOfWeekCronSchedule(string expression)
        {
            Initialize(expression, 0, 6);
        }
    }
}
