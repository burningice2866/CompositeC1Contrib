using System;
using System.Web;
using System.Web.UI;

using C1UrlUtils = Composite.Core.WebClient.UrlUtils;

namespace CompositeC1Contrib.Web
{
    public class MediaUrlFilterModule : IHttpModule
    {
        private static readonly string _mediaUrlPrefix = C1UrlUtils.PublicRootPath + "/media/";
        private static readonly string _handlerUrlPrefix = C1UrlUtils.PublicRootPath + "/Renderers/ShowMedia.ashx?id=";

        public void Init(HttpApplication app)
        {
            app.BeginRequest += new EventHandler(ctx_BeginRequest);
            app.PostMapRequestHandler += new EventHandler(ctx_PostMapRequestHandler);
        }

        public void Dispose() { }

        private void ctx_PostMapRequestHandler(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            if (ctx.Handler is Page && !ctx.Request.Url.LocalPath.StartsWith(C1UrlUtils.AdminRootPath))
            {
                ctx.Response.Filter = new MediaUrlFilter(ctx.Response.Filter, ctx);
            }
        }

        private void ctx_BeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;
            string pathAndQuery = ctx.Request.Url.PathAndQuery;

            if (pathAndQuery.StartsWith(_mediaUrlPrefix))
            {
                if (AppSettings.UseFolderPathsForMediaUrls)
                {
                    ctx.RewritePath(_handlerUrlPrefix + "MediaArchive:/" + pathAndQuery.Substring(_mediaUrlPrefix.Length));
                }
                else
                {
                    string guid = pathAndQuery.Substring(_mediaUrlPrefix.Length, 36);
                    ctx.RewritePath(_handlerUrlPrefix + guid);
                }
            }
        }
    }
}
