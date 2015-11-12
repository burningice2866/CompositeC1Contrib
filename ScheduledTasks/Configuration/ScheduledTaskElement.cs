using System.Configuration;

namespace CompositeC1Contrib.ScheduledTasks.Configuration
{
    public class ScheduledTaskElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("method", IsRequired = true)]
        public string Method
        {
            get { return (string)this["method"]; }
            set { this["method"] = value; }
        }

        [ConfigurationProperty("cronExpression", IsRequired = true)]
        public string CronExpression
        {
            get { return (string)this["cronExpression"]; }
            set { this["cronExpression"] = value; }
        }

        [ConfigurationProperty("arguments", IsDefaultCollection = false, IsRequired = false)]
        public ScheduledTaskMethodArgumentCollection Arguments
        {
            get { return (ScheduledTaskMethodArgumentCollection)this["arguments"]; }
            set { this["arguments"] = value; }
        }
    }
}