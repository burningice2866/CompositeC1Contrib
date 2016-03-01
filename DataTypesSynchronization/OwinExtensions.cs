using System;

using Owin;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribDataTypesSynchronization(this IAppBuilder app, Action<DataTypesSynchronizationConfiguration> configurationCallback)
        {
            var options = new DataTypesSynchronizationConfiguration();

            if (configurationCallback != null)
            {
                configurationCallback(options);
            }
        }
    }
}
