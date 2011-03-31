using System.Configuration;

namespace CompositeC1Contrib
{
    public static class AppSettings
    {
        public static bool UseBetterUrls
        {
            get
            {
                var b = true;
                if (bool.TryParse(ConfigurationManager.AppSettings["useBetterUrls"], out b))
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
    }
}
