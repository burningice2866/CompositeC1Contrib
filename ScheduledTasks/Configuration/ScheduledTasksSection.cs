using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

                var methods = type.GetMethods().Where(m => m.Name == element.Method).ToList();
                if (!methods.Any())
                {
                    throw new ArgumentException(String.Format("Method '{0}' doesn't exist on type {1}", element.Method, type.FullName));
                }

                var methodToCall = FindMethodToCall(methods);
                if (methodToCall == null)
                {
                    throw new ArgumentException(String.Format("Method '{0}' doesn't have a valid signature. It needs to be parameterless or take a 'IJobCancellationToken'", element.Method));
                }

                Expression<Action> action;
                if (methodToCall.GetParameters().Any())
                {
                    var argument = Expression.Constant(JobCancellationToken.Null, typeof(IJobCancellationToken));

                    action = Expression.Lambda<Action>(Expression.Call(methodToCall, argument));
                }
                else
                {
                    action = Expression.Lambda<Action>(Expression.Call(methodToCall));
                }

                RecurringJob.AddOrUpdate(element.Name, action, element.CronExpression);
            }
        }

        private static MethodInfo FindMethodToCall(IEnumerable<MethodInfo> methods)
        {
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof (IJobCancellationToken))
                {
                    return method;
                }

                if (parameters.Length == 0)
                {
                    return method;
                }
            }

            return null;
        }
    }
}
