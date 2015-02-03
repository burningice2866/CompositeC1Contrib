using System;

using Hangfire.Logging;

using C1Log = Composite.Core.Log;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class C1Logger : ILog
    {
        private readonly string _title;

        public C1Logger(string name)
        {
            _title = name;
        }

        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
        {
            if (messageFunc != null)
            {
                var message = messageFunc();

                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug: C1Log.LogVerbose(_title, message); break;

                    case LogLevel.Fatal:
                    case LogLevel.Error:

                        C1Log.LogError(_title, message);

                        if (exception != null)
                        {
                            C1Log.LogError(_title, exception);
                        }

                        break;

                    case LogLevel.Info: C1Log.LogInformation(_title, message); break;

                    case LogLevel.Warn:

                        C1Log.LogWarning(_title, message);

                        if (exception != null)
                        {
                            C1Log.LogWarning(_title, exception);
                        }

                        break;
                }
            }

            return true;
        }
    }
}
