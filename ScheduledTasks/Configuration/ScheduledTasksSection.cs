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

                var arguments = element.Arguments.Cast<ScheduledTaskMethodArgumentElement>().ToDictionary(a => a.Name, a => a.Value);

                var methodToCall = FindMethodToCall(methods, arguments);
                if (methodToCall == null)
                {
                    throw new ArgumentException(String.Format("Method '{0}' doesn't have a valid signature. It needs to be parameterless or take a 'IJobCancellationToken'", element.Method));
                }

                var action = GetMethodExpression(methodToCall, arguments);

                RecurringJob.AddOrUpdate(element.Name, action, element.CronExpression);
            }
        }

        private static Expression<Action> GetMethodExpression(MethodInfo methodToCall, IDictionary<string, string> arguments)
        {
            var parameters = methodToCall.GetParameters();

            if (!parameters.Any())
            {
                return Expression.Lambda<Action>(Expression.Call(methodToCall));
            }

            var argumentExpressions = new List<ConstantExpression>();

            foreach (var param in parameters)
            {
                object value;

                if (!arguments.ContainsKey(param.Name) && param.ParameterType == typeof(IJobCancellationToken))
                {
                    value = JobCancellationToken.Null;
                }
                else
                {
                    var valueAsString = arguments[param.Name];

                    if (param.ParameterType.IsEnum)
                    {
                        value = Enum.Parse(param.ParameterType, valueAsString);
                    }
                    else
                    {
                        value = Convert.ChangeType(valueAsString, param.ParameterType);
                    }
                }

                argumentExpressions.Add(Expression.Constant(value, param.ParameterType));
            }

            return Expression.Lambda<Action>(Expression.Call(methodToCall, argumentExpressions));
        }

        private static MethodInfo FindMethodToCall(IEnumerable<MethodInfo> methods, IDictionary<string, string> arguments)
        {
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var parameterNames = parameters.Select(p => p.Name);
                var argumentNames = arguments.Keys.ToList();

                if (parameters.Length == (arguments.Count + 1)
                    && argumentNames.All(n => parameterNames.Contains(n))
                    && parameters.Any(p => p.ParameterType == typeof(IJobCancellationToken))
                    )
                {
                    return method;
                }

                if (parameters.Length == arguments.Count
                    && argumentNames.All(n => parameterNames.Contains(n)))
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
