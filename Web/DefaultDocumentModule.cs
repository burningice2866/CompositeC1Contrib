using System;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CompositeC1Contrib.Web
{
    public class DefaultDocumentModule : IHttpModule
    {
        public static bool IsDefaultDocumentUrl(string url)
        {
            return url == "/" || url.StartsWith("/default.aspx", StringComparison.OrdinalIgnoreCase);
        }

        private void app_BeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            string path = ctx.Request.RawUrl.ToLower();
            if (IsDefaultDocumentUrl(path))
            {
                var ci = CultureInfo.CurrentCulture;
                var provider = SiteMap.Provider;                
                var node = default(SiteMapNode);

                var cookie = ctx.Request.Cookies["dotgl_lang"];
                if (cookie != null)
                {
                    string val = cookie.Value;
                    try { ci = CultureInfo.GetCultureInfo(val); }
                    catch (ArgumentException) { }
                }

                var baseProvider = provider as BaseSiteMapProvider;
                if (baseProvider != null)
                {
                    node = baseProvider.GetRootNodes().FirstOrDefault(n => n.Culture.Equals(ci));
                }
                else
                {
                    node = provider.RootNode;
                }

                if (node != null && !IsDefaultDocumentUrl(node.Url))
                {
                    ctx.Response.StatusCode = 301;
                    ctx.Response.StatusDescription = "301 Moved Permanently";
                    ctx.Response.AppendHeader("Location", node.Url);
                    ctx.Response.End();
                }
            }
        }

        void IHttpModule.Init(HttpApplication app)
        {
            app.BeginRequest += new EventHandler(app_BeginRequest);
        }

        void IHttpModule.Dispose() { }
    }
}