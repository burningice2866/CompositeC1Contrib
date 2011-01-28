using System;
using System.Globalization;
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

            DataEventSystemFacade.SubscribeToDataBeforeAdd<IPage>(CompositeC1SiteMapProvider.DataBeforeAdd, true);
        }

        private void app_BeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            string extension = Path.GetExtension(ctx.Request.Url.LocalPath);
            if (String.IsNullOrEmpty(extension))
            {
                var url = ctx.Request.RawUrl.ToLower();
                var provider = (BaseSiteMapProvider)SiteMap.Provider;
                var ci = DataLocalizationFacade.ActiveLocalizationCultures.SingleOrDefault(c => url.StartsWith("/" + c.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));

                if (ci == null)
                {
                    ci = CultureInfo.CurrentCulture;
                }

                var node = provider.FindSiteMapNode(url, ci) as CompositeC1SiteMapNode;
                if (node != null)
                {
                    ctx.RewritePath(node.PageNode.Url, ctx.Request.Path, ctx.Request.QueryString.ToString());
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
