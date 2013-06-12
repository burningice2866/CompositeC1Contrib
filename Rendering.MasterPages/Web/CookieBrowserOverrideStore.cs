using System;
using System.Web;

namespace CompositeC1Contrib.Web
{
    public class CookieBrowserOverrideStore : BrowserOverrideStore
    {
        private readonly int _daysToExpire;
        internal static readonly string BrowserOverrideCookieName = "ASPXBrowserOverride";

        public CookieBrowserOverrideStore() : this(7) { }

        public CookieBrowserOverrideStore(int daysToExpire)
        {
            this._daysToExpire = daysToExpire;
        }

        public override string GetOverriddenUserAgent(HttpContext httpContext)
        {
            var cookies = httpContext.Response.Cookies;
            var allKeys = cookies.AllKeys;

            for (int i = 0; i < allKeys.Length; i++)
            {
                if (String.Equals(allKeys[i], BrowserOverrideCookieName, StringComparison.OrdinalIgnoreCase))
                {
                    var cookie = cookies[BrowserOverrideCookieName];
                    if (cookie.Value != null)
                    {
                        return cookie.Value;
                    }

                    return null;
                }
            }

            var cookie2 = httpContext.Request.Cookies[BrowserOverrideCookieName];
            if (cookie2 != null)
            {
                return cookie2.Value;
            }

            return null;
        }

        public override void SetOverriddenUserAgent(HttpContext httpContext, string userAgent)
        {
            var cookie = new HttpCookie(BrowserOverrideCookieName, HttpUtility.UrlEncode(userAgent));
            if (userAgent == null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1.0);
            }
            else if (this._daysToExpire > 0)
            {
                cookie.Expires = DateTime.Now.AddDays((double)this._daysToExpire);
            }

            httpContext.Response.Cookies.Remove(BrowserOverrideCookieName);
            httpContext.Response.Cookies.Add(cookie);
        }
    }
}
