using System;
using System.Linq.Expressions;

using CompositeC1Contrib.ScheduledTasks.Configuration;

using Hangfire;
using Hangfire.CompositeC1;
using Hangfire.Dashboard;

using Owin;

namespace CompositeC1Contrib.ScheduledTasks
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribScheduledTasks(this IAppBuilder app)
        {
            app.UseHangfire(config =>
            {
                config.UseAuthorizationFilters(new IAuthorizationFilter[] {new CompositeC1AuthorizationFilter()});
                config.UseCompositeC1Storage();
                config.UseServer();
            });

            var section = ScheduledTasksSection.GetSection();
            foreach (ScheduledTaskElement element in section.Tasks)
            {
                var type = Type.GetType(element.Type);
                var method = type.GetMethod(element.Method);

                var action = Expression.Lambda<Action>(Expression.Call(method));

                RecurringJob.AddOrUpdate(element.Name, action, element.CronExpression);
            }
        }
    }
}
