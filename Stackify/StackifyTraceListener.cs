using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

using Composite.C1Console.Events;
using Composite.Core.Configuration;

using StackifyLib;
using StackifyLib.Internal.Metrics;
using StackifyLib.Models;
using StackifyLib.Utils;

using StackifyLogger = StackifyLib.Logger;

namespace CompositeC1Contrib.Stackify
{
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class StackifyTraceListener : CustomTraceListener
    {
        public StackifyTraceListener()
        {
            StackifyAPILogger.Log("Composite C1 TraceListener created");
        }

        static StackifyTraceListener()
        {
            GlobalEventSystemFacade.SubscribeToPrepareForShutDownEvent(args =>
            {
                StackifyAPILogger.Log("Composite C1 TraceListener shutting down");
                MetricClient.StopMetricsQueue("Composite C1 TraceListener shutting down");

                StackifyLogger.Shutdown();
            });
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            var entry = (LogEntry)((ICloneable)data).Clone();

            entry.Title = SanatizeTitle(entry.Title);

            var stackifyEvent = Translate(eventType, entry);

            StackifyLogger.QueueLogObject(stackifyEvent);
        }

        private static LogMsg Translate(TraceEventType eventType, LogEntry entry)
        {
            var stackifyEvent = new LogMsg
            {
                Level = MapLevel(eventType),
                Msg = entry.Title + ": " + entry.Message,
                data = HelperFunctions.SerializeDebugData(entry, true),

                AppDetails = new LogMsgGroup
                {
                    AppName = GlobalSettingsFacade.ApplicationName,
                    AppNameID = InstallationInformationFacade.InstallationId,
                    ServerName = entry.MachineName,
                },

                Tags = new List<string>
                {
                    GlobalSettingsFacade.ApplicationName,
                    InstallationInformationFacade.InstallationId.ToString()
                }
            };

            if (entry.ExtendedProperties.ContainsKey("Exception"))
            {
                var exc = entry.ExtendedProperties["Exception"] as Exception;
                if (exc != null)
                {
                    stackifyEvent.Ex = StackifyError.New(exc);
                    stackifyEvent.Msg = entry.Title + ": " + exc.Message;
                }
            }

            return stackifyEvent;
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

        private static string MapLevel(TraceEventType type)
        {
            switch (type)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error: return "ERROR";

                case TraceEventType.Warning: return "WARNING";
                case TraceEventType.Information: return "INFORMATION";
                case TraceEventType.Verbose: return "VERBOSE";

                default: return "NOTICE";
            }
        }
    }
}