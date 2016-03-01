using System;
using System.IO;

using Composite.Core.IO;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public class Logger : IDisposable
    {
        private const string LogPath = "~/App_Data/DataTypesSynchronization/Logs/{0}.txt";

        private readonly StreamWriter _log;

        public Logger(Guid jobId)
        {
            var path = PathUtil.Resolve(String.Format(LogPath, jobId));

            DirectoryUtils.EnsurePath(path);

            if (!C1File.Exists(path))
            {
                C1File.WriteAllText(path, "Job started");
            }

            var fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);

            _log = new StreamWriter(fs);
        }

        public void LogException(Type t, Exception exc)
        {
            AppendToLog("Error when updating datatype: '{0}'\nException:{1}", t.FullName, exc);
        }

        public void AppendToLog(string line, params object[] args)
        {
            line = String.Format(line, args);

            AppendToLog(line);
        }

        public void AppendToLog(string line)
        {
            line = String.Format("{0}: {1}", DateTime.UtcNow.ToLongTimeString(), line);

            _log.WriteLine(line);
            _log.Flush();
        }

        public void Dispose()
        {
            if (_log != null)
            {
                _log.Close();
                _log.Dispose();
            }
        }

        public static string ReadLog(Guid jobId)
        {
            var path = PathUtil.Resolve(String.Format(LogPath, jobId));
            if (String.IsNullOrEmpty(path) || !C1File.Exists(path))
            {
                return String.Empty;
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
