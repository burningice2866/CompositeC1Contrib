using System;

using Composite.Core.Configuration;

using Loggly;
using Loggly.Config;

namespace CompositeC1Contrib.Loggly
{
    public static class LogglyFacade
    {
        private static readonly ILogglyClient Client;

        static LogglyFacade() 
        {
            var instance = LogglyConfig.Instance;

            if (String.IsNullOrEmpty(instance.ApplicationName))
            {
                instance.ApplicationName = GlobalSettingsFacade.ApplicationName;
            }

            var complexTags = instance.Tags.ComplexTags;

            complexTags.Add(new ApplicationNameTag
            {
                Formatter = "application-{0}"
            });

            complexTags.Add(new InstallationIdTag
            {
                Formatter = "installation-{0}"
            });

            Client = new LogglyClient();
        }

        public static void Log(LogglyEvent logglyEvent)
        {
            Client.Log(logglyEvent);
        }
    }
}
