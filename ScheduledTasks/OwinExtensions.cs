using System;
using System.Linq.Expressions;

using CompositeC1Contrib.ScheduledTasks.Configuration;

using Hangfire;
using Hangfire.CompositeC1;
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

            var configuration = GlobalConfiguration.Configuration;

            configuration.UseStorage(new CompositeC1Storage());

            var options = new BackgroundJobServerOptions();

            if (workerCount.HasValue)
            {
                options.WorkerCount = workerCount.Value;
            }

            app.UseHangfireServer(options);

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                AuthorizationFilters = new[] { new CompositeC1AuthorizationFilter() }
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
