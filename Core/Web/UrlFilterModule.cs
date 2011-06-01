using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;

using C1UrlUtils = Composite.Core.WebClient.UrlUtils;

namespace CompositeC1Contrib.Web
{
    public class UrlFilterModule : IHttpModule
    {
        void IHttpModule.Dispose() { }

        void IHttpModule.Init(HttpApplication app)
        {
            app.BeginRequest += OnBeginRequest;
            app.PostRequestHandlerExecute += OnPostRequestHandlerExecute;
        }

        protected virtual void RewritePath(HttpContext ctx)
        {
            string path = ctx.Request.Url.LocalPath;
            string extension = Path.GetExtension(path);

            if (!path.StartsWith(C1UrlUtils.AdminRootPath, StringComparison.OrdinalIgnoreCase) 
                && (UrlUtils.IsDefaultDocumentUrl(path) || String.IsNullOrEmpty(extension) || String.Equals(extension, ".aspx", StringComparison.OrdinalIgnoreCase)))
            {
                var pathInfo = new StringBuilder();
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
                            pathInfo.Insert(0, "/" + localPath.Substring(lastIndex, localPath.Length - lastIndex));
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

                    if (pathInfo.Length == 0)
                    {
                        pathInfo.Append(ctx.Request.PathInfo);
                    }

                    string nodeUrl = node.PageNode.Url;
                    if (nodeUrl.Contains("?"))
                    {
                        nodeUrl = nodeUrl.Substring(0, nodeUrl.IndexOf("?"));
                    }

                    ctx.RewritePath(nodeUrl, pathInfo.ToString(), query);
                }
            }
        }

        protected virtual bool ShouldRewriteUrls(HttpContext ctx)
        {
            return ctx.Handler is Page && !ctx.Request.Url.LocalPath.StartsWith(C1UrlUtils.AdminRootPath);
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;
            RewritePath(ctx);
        }

        private void OnPostRequestHandlerExecute(object sender, EventArgs e)
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
