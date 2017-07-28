using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using System.Web;

using Composite;

namespace CompositeC1Contrib.Localization
{
    public class C1Res
    {
        private static readonly ConcurrentDictionary<string, ResourceManager> Managers = new ConcurrentDictionary<string, ResourceManager>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The localized string or 'key' if it doesn't exist</returns>
        public static IHtmlString THtml(string key)
        {
            Verify.ArgumentNotNull(key, nameof(key));

            var value = T(key);

            return new HtmlString(value);
        }

        [Obsolete("Use the TFormat(string, object[]) overload")]
        public static string TFormat(string format, string key, params object[] args)
        {
            Verify.ArgumentNotNull(key, nameof(key));

            return TFormat(key, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns>The localized string or 'key' if it doesn't exist</returns>
        public static string TFormat(string key, params object[] args)
        {
            Verify.ArgumentNotNull(key, nameof(key));

            var format = T(key);
            if (format == null || format == key)
            {
                return key;
            }

            return String.Format(format, args);
        }

        public static string T(string key)
        {
            Verify.ArgumentNotNull(key, nameof(key));

            return T(key, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resourceSet"></param>
        /// <returns>The localized string or 'key' if it doesn't exist</returns>
        public static string T(string key, string resourceSet)
        {
            Verify.ArgumentNotNull(key, nameof(key));

            return T(key, resourceSet, CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="culture"></param>
        /// <returns>The localized string or 'key' if it doesn't exist</returns>
        public static string T(string key, CultureInfo culture)
        {
            Verify.ArgumentNotNull(key, nameof(key));

            return T(key, String.Empty, culture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resourceSet"></param>
        /// <param name="culture"></param>
        /// <returns>The localized string or 'key' if it doesn't exist</returns>
        public static string T(string key, string resourceSet, CultureInfo culture)
        {
            Verify.ArgumentNotNull(key, nameof(key));

            var value = GetResourceManager(resourceSet).GetObject(key, culture) as string;

            return value ?? key;
        }

        public static ResourceManager GetResourceManager(string resourceSet)
        {
            return Managers.GetOrAdd(resourceSet, _ => new C1ResourceManager(_));
        }

        public static ResourceSet GetResourceSet(string resourceSet)
        {
            var culture = CultureInfo.CurrentUICulture;

            return GetResourceManager(resourceSet).GetResourceSet(culture, false, true);
        }
    }
}
