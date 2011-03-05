using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

using Composite.Data;

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

        private void app_BeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            string path = ctx.Request.Url.LocalPath;
            string extension = Path.GetExtension(path);

            if (UrlUtils.IsDefaultDocumentUrl(path) || String.IsNullOrEmpty(extension))
            {
                var provider = (CompositeC1SiteMapProvider)SiteMap.Provider;
                var ci = DataLocalizationFacade.ActiveLocalizationCultures.SingleOrDefault(c => path.StartsWith("/" + c.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));

                if (ci == null)
                {
                    ci = DataLocalizationFacade.DefaultLocalizationCulture;
                }

                if (ctx.Request.QueryString["dataScope"] == "administrated")
                {
                    path += "?dataScope=administrated";
                }

                var node = provider.FindSiteMapNode(path, ci) as CompositeC1SiteMapNode;
                if (node == null)
                {
                    if (UrlUtils.IsDefaultDocumentUrl(path))
                    {
                        node = provider.RootNode as CompositeC1SiteMapNode;
                    }
                }

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

        private void app_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            if (ctx.Handler is CompositeC1Page 
                && ctx.Response.StatusCode != (int)HttpStatusCode.InternalServerError)
            {
                ctx.Response.Filter = new UrlFilter(ctx.Response.Filter, ctx);
            }
        }
    }
}
