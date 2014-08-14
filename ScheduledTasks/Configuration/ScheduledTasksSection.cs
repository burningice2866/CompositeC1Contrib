using System.Configuration;

namespace CompositeC1Contrib.ScheduledTasks.Configuration
{
    public class ScheduledTasksSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public ScheduledTaskCollection Tasks
        {
            get { return (ScheduledTaskCollection)this[""]; }
            set { this[""] = value; }
        }

        public static ScheduledTasksSection GetSection()
        {
            return ConfigurationManager.GetSection("compositeC1Contrib/scheduledTasks") as ScheduledTasksSection;
        }
    }
}
