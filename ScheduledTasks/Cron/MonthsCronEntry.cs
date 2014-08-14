namespace CompositeC1Contrib.ScheduledTasks.Cron
{
    public class MonthsCronSchedule : CronScheduleBase
    {
        public MonthsCronSchedule(string expression)
        {
            Initialize(expression, 1, 12);
        }
    }
}
