using System;
using System.IO;

using Composite.Core;
using Composite.Core.IO;

namespace CompositeC1Contrib.ECommerce
{
    public static class ECommerceLog
    {
        private static readonly object SyncRoot = new object();
        private static readonly string LogFile = Path.Combine(ECommerce.RootPath, "log.txt");

        public static void WriteLog(string message)
        {
            WriteLog(message, null);
        }

        public static void WriteLog(string message, Exception exc)
        {
            lock (SyncRoot)
            {
                using (var writer = GetOrCreateFile(LogFile))
                {
                    if (exc != null)
                    {
                        Log.LogError(message, exc);

                        message += ", Exception: " + exc;
                    }

                    writer.Write(DateTime.UtcNow);
                    writer.Write("\t");
                    writer.Write(message);
                    writer.Write(Environment.NewLine);
                }
            }
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!C1Directory.Exists(path))
            {
                lock (SyncRoot)
                {
                    if (!C1Directory.Exists(path))
                    {
                        C1Directory.CreateDirectory(path);
                    }
                }
            }
        }

        private static C1StreamWriter GetOrCreateFile(string path)
        {
            if (!C1File.Exists(path))
            {
                lock (SyncRoot)
                {
                    if (!C1File.Exists(path))
                    {
                        return C1File.CreateText(path);
                    }
                }
            }

            return C1File.AppendText(path);
        }
    }
}
