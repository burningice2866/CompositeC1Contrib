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

                if (ri == null || ri.PageUrl == null)
                {
                    ctx.Items[key] = ri = new RequestInfo(ctx);
                }

                return ri;
            }
        }

        public NameValueCollection ForeignQueryStringParameters { get; private set; }
        public PageUrl PageUrl { get; private set; }

        private RequestInfo(HttpContext ctx)
        {
            NameValueCollection _foreignQueryStringParameters;

            PageUrl = PageUrl.Parse(ctx.Request.Url.OriginalString, out _foreignQueryStringParameters);
            ForeignQueryStringParameters = _foreignQueryStringParameters;
        }
    }
}
