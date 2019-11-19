using System;

using Composite.Core;

using Hangfire;

using Microsoft.Extensions.DependencyInjection;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class C1CMSContainerJobActivator : JobActivator
    {
        public override object ActivateJob(Type type)
        {
            return ServiceLocator.GetService(type) ?? ActivatorUtilities.CreateInstance(ServiceLocator.ServiceProvider, type);
        }
    }
}
