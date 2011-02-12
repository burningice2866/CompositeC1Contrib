using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;

using Composite.Data;
using Composite.Data.Types;

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

            string path = ctx.Request.RawUrl.ToLower();
            string extension = Path.GetExtension(ctx.Request.Url.LocalPath);

            if (DefaultDocumentModule.IsDefaultDocumentUrl(path) || String.IsNullOrEmpty(extension))
            {
                var provider = (BaseSiteMapProvider)SiteMap.Provider;
                var ci = DataLocalizationFacade.ActiveLocalizationCultures.SingleOrDefault(c => path.StartsWith("/" + c.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));

                if (ci == null)
                {
                    ci = DataLocalizationFacade.DefaultLocalizationCulture;
                }

                var node = provider.FindSiteMapNode(path, ci) as CompositeC1SiteMapNode;
                if (node == null)
                {
                    if (DefaultDocumentModule.IsDefaultDocumentUrl(path))
                    {
                        node = provider.RootNode as CompositeC1SiteMapNode;
                    }
                }

                if (node != null)
                {
                    ctx.RewritePath(node.PageNode.Url, ctx.Request.PathInfo, ctx.Request.QueryString.ToString());
                }
            }
        }

        private void app_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            if (ctx.Response.StatusCode != (int)HttpStatusCode.InternalServerError
                && ctx.Handler is Page
                && RequestInfo.Current.PageUrl != null)
            {
                ctx.Response.Filter = new UrlFilter(ctx.Response.Filter, ctx);
            }
        }
    }
}
