using System.Web;

namespace CompositeC1Contrib.Web
{
    public abstract class BrowserOverrideStore
    {
        public abstract string GetOverriddenUserAgent(HttpContext httpContext);
        public abstract void SetOverriddenUserAgent(HttpContext httpContext, string userAgent);
    }
}
