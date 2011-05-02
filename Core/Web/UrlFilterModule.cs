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
                string pathInfo = String.Empty;
                CompositeC1SiteMapNode node = null;
                var url = ctx.Request.Url;

                var localPath = url.LocalPath;
                string query = url.Query;                

                while (node == null && !String.IsNullOrEmpty(localPath))
                {
                    node = CompositeC1SiteMapProvider.ResolveNodeFromUrl(localPath, query);
                    if (node == null)
                    {
                        int lastIndex = localPath.LastIndexOf('/');
                        if (lastIndex > 0)
                        {
                            pathInfo += "/" + localPath.Substring(lastIndex, localPath.Length - lastIndex);
                            localPath = localPath.Substring(0, lastIndex);
                        }
                        else
                        {
                            localPath = String.Empty;
                        }
                    }
                }

                if (node != null)
                {
                    if (!String.IsNullOrEmpty(query))
                    {
                        query = query.Remove(0, 1);
                    }

                    if (String.IsNullOrEmpty(pathInfo))
                    {
                        pathInfo = ctx.Request.PathInfo;
                    }

                    ctx.RewritePath(node.PageNode.Url, pathInfo, query);
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
