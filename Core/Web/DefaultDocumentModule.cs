using System;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CompositeC1Contrib.Web
{
    public class DefaultDocumentModule : IHttpModule
    {
        private void OnBeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            string path = ctx.Request.RawUrl.ToLower();
            if (UrlUtils.IsDefaultDocumentUrl(path))
            {
                var ci = CultureInfo.CurrentCulture;
                var provider = (BaseSiteMapProvider)SiteMap.Provider;                

                var cookie = ctx.Request.Cookies["dotgl_lang"];
                if (cookie != null)
                {
                    string val = cookie.Value;
                    try { ci = CultureInfo.GetCultureInfo(val); }
                    catch (ArgumentException) { }
                }

                var node = provider.GetRootNodes().FirstOrDefault(n => n.Culture.Equals(ci));
                if (node != null && !UrlUtils.IsDefaultDocumentUrl(node.Url))
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
            app.BeginRequest += OnBeginRequest;
        }

        void IHttpModule.Dispose() { }
    }
}