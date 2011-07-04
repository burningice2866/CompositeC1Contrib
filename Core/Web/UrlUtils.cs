using System;

using C1UrlUtils = Composite.Core.WebClient.UrlUtils;

namespace CompositeC1Contrib.Web
{
    public static class UrlUtils
    {
        public static bool IsDefaultDocumentUrl(string url)
        {
            return url == "/"
            || url == C1UrlUtils.PublicRootPath
            || url.StartsWith("/?")
            || url.StartsWith("/default.aspx", StringComparison.OrdinalIgnoreCase);
        }
    }
}
