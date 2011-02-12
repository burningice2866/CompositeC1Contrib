using System.Collections.Specialized;
using System.Web;

using Composite.Data;

namespace CompositeC1Contrib.Web
{
    public class RequestInfo
    {
        private const string key = "___RI___";

        public static RequestInfo Current
        {
            get
            {
                var ctx = HttpContext.Current;
                var ri = ctx.Items[key] as RequestInfo;

                if (ri == null)
                {
                    ctx.Items[key] = ri = new RequestInfo(ctx);
                }

                return ri;
            }
        }

        public bool IsPreview
        {
            get
            {
                var ctx = HttpContext.Current;

                return ctx.Items.Contains("SelectedPage") && ctx.Items.Contains("SelectedContents");
            }
        }

        private RequestInfo(HttpContext ctx)
        {
            
        }
    }
}
