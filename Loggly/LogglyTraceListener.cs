using System;
using System.Diagnostics;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

using Loggly;
using Loggly.Transports.Syslog;

namespace CompositeC1Contrib.Loggly
{
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class LogglyTraceListener : CustomTraceListener
    {
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            var entry = (LogEntry)((ICloneable)data).Clone();

            entry.Title = SanatizeTitle(entry.Title);

            var logglyEvent = Translate(eventType, entry);

            LogglyFacade.Log(logglyEvent);
        }

        private static LogglyEvent Translate(TraceEventType eventType, LogEntry entry)
        {
            var logglyEvent = new LogglyEvent
            {
                Syslog =
                {
                    Level = MapLevel(eventType)
                }
            };

            logglyEvent.Data.Add("C1_Logmessage", entry);

            return logglyEvent;
        }

        private static string SanatizeTitle(string title)
        {
            if (!title.Contains(")"))
            {
                return title;
            }

            var index = title.LastIndexOf(")", StringComparison.Ordinal);

            return title.Substring(index + 1).Trim();
        }

        public override void Write(string message) { }
        public override void WriteLine(string message) { }

        private static Level MapLevel(TraceEventType type)
        {
            switch (type)
            {
                case TraceEventType.Critical: return Level.Critical;
                case TraceEventType.Error: return Level.Error;
                case TraceEventType.Warning: return Level.Warning;
                case TraceEventType.Information: return Level.Information;
                case TraceEventType.Verbose: return Level.Debug;

                default: return Level.Notice;
            }
        }
    }
}