using System;

using Composite.Core;

using Hangfire;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class C1CMSContainerJobActivator : JobActivator
    {
        public override object ActivateJob(Type type)
        {
            return ServiceLocator.GetService(type);
        }
    }
}
