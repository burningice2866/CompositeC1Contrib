using System.Configuration;

namespace CompositeC1Contrib
{
    public static class AppSettings
    {
        public static bool UseNicerUrls
        {
            get
            {
                var b = true;
                if (bool.TryParse(ConfigurationManager.AppSettings["useNicerUrls"], out b))
                {
                    return b;
                }

                return true;
            }
        }

        public static bool UseExtensionlessUrls
        {
            get
            {
                var b = true;
                if (bool.TryParse(ConfigurationManager.AppSettings["useExtensionlessUrls"], out b))
                {
                    return b;
                }

                return true;
            }
        }

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
