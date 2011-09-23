using System.Configuration;

namespace CompositeC1Contrib
{
    public static class AppSettings
    {
        public static bool UseFolderPathsForMediaUrls
        {
            get
            {
                var b = true;
                if (bool.TryParse(ConfigurationManager.AppSettings["useFolderPathsForMediaUrls"], out b))
                {
                    return b;
                }

                return true;
            }
        }
    }
}
