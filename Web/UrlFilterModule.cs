using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;

using Composite.Data;

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

        void app_BeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            string extension = Path.GetExtension(ctx.Request.Url.LocalPath);
            if (String.IsNullOrEmpty(extension))
            {
                var url = ctx.Request.RawUrl.ToLower();

                var ci = DataLocalizationFacade.ActiveLocalizationCultures.SingleOrDefault(c => url.StartsWith("/" + c.TwoLetterISOLanguageName));
                if (ci != null)
                {
                    var provider = (BaseSiteMapProvider)SiteMap.Provider;

                    var node = provider.FindSiteMapNode(url, ci) as CompositeC1SiteMapNode;
                    if (node != null)
                    {
                        ctx.RewritePath(node.PageNode.Url, ctx.Request.Path, ctx.Request.QueryString.ToString());

                        return;
                    }
                }
            }
        }

        void app_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var ctx = app.Context;

            if (ctx.Response.StatusCode != (int)HttpStatusCode.InternalServerError
                && ctx.Handler is Page
                && RequestInfo.Current.PageUrl != null)
            {
                ctx.Response.Filter = new UrlFilter(ctx.Response.Filter, ctx);
            }
        }
    }
}
