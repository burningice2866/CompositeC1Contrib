using System;
using System.IO;

namespace CompositeC1Contrib.ScheduledTasks
{
    public static class Logger
    {
        private static readonly string LogFile = Path.Combine(ScheduledTasksWorkflow.RootPath, "log.txt");
        private static readonly string TaskLogFilePattern = Path.Combine(ScheduledTasksWorkflow.RootPath, "log.{0}.txt");

        public static void AppendLogMessage(string message, params object[] arg)
        {
            Log(message, arg, LogFile);
        }

        public static void AppendLogMessage(ScheduledTask task, string message, params object[] arg)
        {
            var logFile = String.Format(TaskLogFilePattern, task.Name);

            Log(message, arg, logFile);
        }

        private static void Log(string message, object[] arg, string logFile)
        {
            lock (FileUtils.SyncRoot)
            {
                using (var writer = FileUtils.GetOrCreateFile(logFile))
                {
                    writer.Write(DateTime.Now);
                    writer.Write("\t");
                    writer.Write(message, arg);
                    writer.Write(Environment.NewLine);
                }
            }
        }
    }

    public static class LoggerExtensionMethod
    {
        public static void AppendLogMessage(this ScheduledTask task, string message, params object[] arg)
        {
            Logger.AppendLogMessage(task, message, arg);
        }
    }
}
