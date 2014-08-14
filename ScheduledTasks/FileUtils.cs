using System.IO;

namespace CompositeC1Contrib.ScheduledTasks
{
    public static class FileUtils
    {
        public static readonly object SyncRoot = new object();

        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                lock (SyncRoot)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }

        public static StreamWriter GetOrCreateFile(string path)
        {
            if (!File.Exists(path))
            {
                lock (SyncRoot)
                {
                    if (!File.Exists(path))
                    {
                        return File.CreateText(path);
                    }
                }
            }

            return File.AppendText(path);
        }
    }
}
