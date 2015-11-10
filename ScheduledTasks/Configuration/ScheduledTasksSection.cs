using System;
using System.Configuration;
using System.Linq.Expressions;

using Hangfire;

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

        public static void EnsureJobsFromConfig()
        {
            var section = GetSection();
            if (section == null)
            {
                return;
            }

            foreach (ScheduledTaskElement element in section.Tasks)
            {
                var type = Type.GetType(element.Type);
                if (type == null)
                {
                    throw new ArgumentException(String.Format("Type '{0}' doesn't exist", element.Type));
                }

                var method = type.GetMethod(element.Method);
                if (method == null)
                {
                    throw new ArgumentException(String.Format("Method '{0}' doesn't exist on type {1}", element.Method, type.FullName));
                }

                var action = Expression.Lambda<Action>(Expression.Call(method));

                RecurringJob.AddOrUpdate(element.Name, action, element.CronExpression);
            }
        }
    }
}
