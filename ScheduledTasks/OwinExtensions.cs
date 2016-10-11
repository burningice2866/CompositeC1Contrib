using System;
using System.Linq;

using CompositeC1Contrib.Composition;
using CompositeC1Contrib.ScheduledTasks.Configuration;

using Hangfire;
using Hangfire.Server;

using Owin;

namespace CompositeC1Contrib.ScheduledTasks
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribScheduledTasks(this IAppBuilder app, Action<ScheduledTasksConfiguration> configurationCallback)
        {
            var options = new ScheduledTasksConfiguration();

            if (configurationCallback != null)
            {
                configurationCallback(options);
            }

            var configuration = GlobalConfiguration.Configuration;

            configuration.UseLogProvider(new C1LogProvider());
            configuration.UseStorage(options.JobStorage);

            var backgroundProcesses = CompositionContainerFacade.GetExportedValues<IBackgroundProcess>().ToArray();

            if (options.ServerOptions != null)
            {
                app.UseHangfireServer(options.ServerOptions, backgroundProcesses);
            }
            else
            {
                app.UseHangfireServer(backgroundProcesses);
            }

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                AppPath = null,
                Authorization = new[] { new CompositeC1AuthorizationFilter() }
            });

            ScheduledTasksSection.EnsureJobsFromConfig();
        }
    }
}
