using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

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
            if (!AppSettings.UseMvcForContentRendering)
            {
                var ctx = ((HttpApplication)sender).Context;
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

        private void app_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            if (AppSettings.UseFriendlyExtensionlessUrls)
            {
                var ctx = ((HttpApplication)sender).Context;

                if ((ctx.Handler is CompositeC1Page || ctx.Handler is MvcHandler)
                    && ctx.Response.StatusCode != (int)HttpStatusCode.InternalServerError)
                {
                    ctx.Response.Filter = new UrlFilter(ctx.Response.Filter, ctx);
                }
            }
        }
    }
}
