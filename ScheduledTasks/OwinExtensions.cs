using System;
using System.Linq.Expressions;

using CompositeC1Contrib.ScheduledTasks.Configuration;

using Hangfire;
using Hangfire.CompositeC1;
using Hangfire.Dashboard;
using Hangfire.Logging;

using Owin;

namespace CompositeC1Contrib.ScheduledTasks
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribScheduledTasks(this IAppBuilder app)
        {
            app.UseCompositeC1ContribScheduledTasks(null);
        }

        public static void UseCompositeC1ContribScheduledTasks(this IAppBuilder app, int? workerCount)
        {
            LogProvider.SetCurrentLogProvider(new C1LogProvider());

            app.UseHangfire(config =>
            {
                config.UseAuthorizationFilters(new IAuthorizationFilter[] { new CompositeC1AuthorizationFilter() });
                config.UseCompositeC1Storage();

                if (workerCount.HasValue)
                {
                    config.UseServer(workerCount.Value);
                }
                else
                {
                    config.UseServer();
                }
            });

            var section = ScheduledTasksSection.GetSection();
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
