using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using System.Web;

namespace CompositeC1Contrib.Localization
{
    public class C1Res
    {
        private static ConcurrentDictionary<string, ResourceManager> _managers = new ConcurrentDictionary<string, ResourceManager>();

        public static IHtmlString THtml(string key)
        {
            return new HtmlString(T(key));
        }

        public static string TFormat(string format, string key, params object[] args)
        {
            return String.Format(T(key), args);
        }

        public static string T(string key)
        {
            return T(key, String.Empty);
        }

        public static string T(string key, string resourceSet)
        {
            return T(key, resourceSet, CultureInfo.CurrentUICulture);
        }

        public static string T(string key, CultureInfo culture)
        {
            return T(key, String.Empty, culture);
        }

        public static string T(string key, string resourceSet, CultureInfo culture)
        {
            var result = GetResourceManager(resourceSet).GetObject(key, culture) as string;
            if (result == null)
            {
                return key;
            }

            return result;
        }

        public static void AddResource(string key, string value)
        {
            var culture = CultureInfo.CurrentUICulture;

            using (var writer = new C1ResourceWriter(culture))
            {
                writer.AddResource(key, value);
            }
        }

        public static ResourceManager GetResourceManager(string resourceSet)
        {
            return _managers.GetOrAdd(resourceSet, _ =>
            {
                return new C1ResourceManager(_);
            });
        }

        public static ResourceSet GetResourceSet(string resourceSet)
        {
            var culture = CultureInfo.CurrentUICulture;

            return GetResourceManager(resourceSet).GetResourceSet(culture, false, true);
        }
    }
}
