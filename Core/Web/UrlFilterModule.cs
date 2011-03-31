using System;
using System.IO;
using System.Net;
using System.Web;

using CompositeC1Contrib.Web.UI;

namespace CompositeC1Contrib.Web
{
    public class UrlFilterModule : IHttpModule
    {
        void IHttpModule.Dispose() { }

        void IHttpModule.Init(HttpApplication app)
        {
            app.BeginRequest += new EventHandler(app_BeginRequest);
            app.PostRequestHandlerExecute += new EventHandler(app_PostRequestHandlerExecute);
        }

        protected virtual void RewritePath(HttpContext ctx)
        {
            string path = ctx.Request.Url.LocalPath;
            string extension = Path.GetExtension(path);

            if (UrlUtils.IsDefaultDocumentUrl(path) || String.IsNullOrEmpty(extension) || String.Equals(extension, ".aspx", StringComparison.OrdinalIgnoreCase))
            {
                var node = CompositeC1SiteMapProvider.ResolveNodeFromUrl(ctx.Request.Url);

                if (node != null)
                {
                    string query = ctx.Request.Url.Query;
                    if (!String.IsNullOrEmpty(query))
                    {
                        query = query.Remove(0, 1);
                    }

                    ctx.RewritePath(node.PageNode.Url, ctx.Request.PathInfo, query);
                }
            }
        }

        protected virtual bool ShouldRewriteUrls(HttpContext ctx)
        {
            return ctx.Handler is CompositeC1Page;       
        }

        private void app_BeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;
            RewritePath(ctx);
        }

        private void app_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            if (ctx.Response.StatusCode != (int)HttpStatusCode.InternalServerError
                && ShouldRewriteUrls(ctx))
            {

                ctx.Response.Filter = new UrlFilter(ctx.Response.Filter, ctx);
            }
        }
    }
}
