using System.Configuration;

namespace CompositeC1Contrib
{
    public static class AppSettings
    {
        public static bool UseMvcForContentRendering
        {
            get
            {
                var b = false;
                if (bool.TryParse(ConfigurationManager.AppSettings["useMvcForContentRendering"], out b))
                {
                    return b;
                }

                return false;
            }
        }

        public static bool UseFriendlyExtensionlessUrls
        {
            get
            {
                var b = true;
                if (bool.TryParse(ConfigurationManager.AppSettings["useFriendlyExtensionlessUrls"], out b))
                {
                    return b;
                }

                return true;
            }
        }
    }
}
