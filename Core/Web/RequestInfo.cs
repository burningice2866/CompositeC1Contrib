using System;
using System.Web;

namespace CompositeC1Contrib.Web
{
    public class RequestInfo
    {
        private const string Key = "___RI___";

        private readonly HttpContext _ctx;

        public static RequestInfo Current
        {
            get
            {
                var ctx = HttpContext.Current;
                var ri = ctx.Items[Key] as RequestInfo;

                if (ri == null)
                {
                    ctx.Items[Key] = ri = new RequestInfo(ctx);
                }

                return ri;
            }
        }

        public string PreviewKey
        {
            get { return _ctx.Request.QueryString["previewKey"]; }
        }

        public bool IsPreview
        {
            get { return !String.IsNullOrEmpty(PreviewKey); }
        }

        private RequestInfo(HttpContext ctx)
        {
            _ctx = ctx;
        }
    }
}
