using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace CompositeC1Contrib.Web
{
    public static class BrowserHelpers
    {
        private sealed class UserAgentWorkerRequest : SimpleWorkerRequest
        {
            private readonly string _userAgent;

            public UserAgentWorkerRequest(string userAgent)
                : base(string.Empty, string.Empty, new StringWriter())
            {
                this._userAgent = userAgent;
            }

            public override string GetKnownRequestHeader(int index)
            {
                if (index != 39)
                {
                    return null;
                }

                return this._userAgent;
            }
        }

        private static readonly object _browserOverrideKey = new object();
        private static readonly object _userAgentKey = new object();

        private const string DesktopUserAgent = "Mozilla/4.0 (compatible; MSIE 6.1; Windows XP)";
        private const string MobileUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows CE; IEMobile 8.12; MSIEMobile 6.0)";

        public static void SetOverriddenBrowser(this HttpContext httpContext, BrowserOverride browserOverride)
        {
            string userAgent = null;
            switch (browserOverride)
            {
                case BrowserOverride.Desktop:

                    if ((httpContext.Request.Browser == null) || httpContext.Request.Browser.IsMobileDevice)
                    {
                        userAgent = DesktopUserAgent;
                    }

                    break;

                case BrowserOverride.Mobile:

                    if ((httpContext.Request.Browser == null) || !httpContext.Request.Browser.IsMobileDevice)
                    {
                        userAgent = MobileUserAgent;
                    }

                    break;
            }

            if (userAgent != null)
            {
                httpContext.SetOverriddenBrowser(userAgent);
            }
            else
            {
                httpContext.ClearOverriddenBrowser();
            }
        }

        public static void ClearOverriddenBrowser(this HttpContext httpContext)
        {
            string userAgent = null;

            httpContext.SetOverriddenBrowser(userAgent);
        }

        public static void SetOverriddenBrowser(this HttpContext httpContext, string userAgent)
        {
            httpContext.Items[_userAgentKey] = userAgent;
            httpContext.Items[_browserOverrideKey] = null;

            BrowserOverrideStores.Current.SetOverriddenUserAgent(httpContext, userAgent);
        }

        public static HttpBrowserCapabilities GetOverriddenBrowser(this HttpContext httpContext)
        {
            return httpContext.GetOverriddenBrowser(new Func<string, HttpBrowserCapabilities>(BrowserHelpers.CreateOverriddenBrowser));
        }

        internal static HttpBrowserCapabilities GetOverriddenBrowser(this HttpContext httpContext, Func<string, HttpBrowserCapabilities> createBrowser)
        {
            var browser = (HttpBrowserCapabilities)httpContext.Items[_browserOverrideKey];
            if (browser == null)
            {
                string overriddenUserAgent = httpContext.GetOverriddenUserAgent();
                if (!String.Equals(overriddenUserAgent, httpContext.Request.UserAgent))
                {
                    browser = createBrowser(overriddenUserAgent);
                }
                else
                {
                    browser = httpContext.Request.Browser;
                }

                httpContext.Items[_browserOverrideKey] = browser;
            }

            return browser;
        }

        public static string GetOverriddenUserAgent(this HttpContext httpContext)
        {
            return (((string)httpContext.Items[_userAgentKey]) ?? (BrowserOverrideStores.Current.GetOverriddenUserAgent(httpContext) ?? httpContext.Request.UserAgent));
        }

        private static HttpBrowserCapabilities CreateOverriddenBrowser(string userAgent)
        {
            return new HttpContext(new UserAgentWorkerRequest(userAgent)).Request.Browser;
        }
    }
}
