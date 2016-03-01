using System;

using Hangfire;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public static class DefaultJobs
    {
        public static void SynchronizeData(Guid jobId, SynchronizationFrequency frequency, IJobCancellationToken cancellationToken)
        {
            var job = new SynchronizationJob(jobId, cancellationToken.ShutdownToken);

            job.UpdateAll(frequency);
        }
    }
}
