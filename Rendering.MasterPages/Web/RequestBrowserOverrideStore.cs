using System.Web;

namespace CompositeC1Contrib.Web
{
    internal sealed class RequestBrowserOverrideStore : BrowserOverrideStore
    {
        public override string GetOverriddenUserAgent(HttpContext httpContext)
        {
            return httpContext.Request.UserAgent;
        }

        public override void SetOverriddenUserAgent(HttpContext httpContext, string userAgent) { }
    }
}
