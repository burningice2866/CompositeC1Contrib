using System;
using System.Linq;

using Composite.C1Console.Elements;

using CompositeC1Contrib.Composition;
using CompositeC1Contrib.ScheduledTasks.C1Console;
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

            configurationCallback?.Invoke(options);

            var configuration = GlobalConfiguration.Configuration;

            configuration.UseActivator(new C1CMSContainerJobActivator());
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

            UrlToEntityTokenFacade.Register(new UrlToEntityTokenMapper());
        }
    }
}
