using System.Web;
using System.Web.Mvc;

namespace CompositeC1Contrib.Web
{
    public class MvcUrlFilterModule : UrlFilterModule
    {
        protected override void RewritePath(HttpContext ctx)
        {
            // do nothing
        }
        
        protected override bool ShouldRewriteUrls(HttpContext ctx)
        {
            return ctx.Handler is MvcHandler;
        }
    }
}
