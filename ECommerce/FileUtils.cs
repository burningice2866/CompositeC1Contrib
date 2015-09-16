using Composite.Core.IO;

namespace CompositeC1Contrib.ECommerce
{
    public static class FileUtils
    {
        public static readonly object SyncRoot = new object();

        public static void CreateDirectoryIfNotExists(string path)
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

        public static C1StreamWriter GetOrCreateFile(string path)
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
