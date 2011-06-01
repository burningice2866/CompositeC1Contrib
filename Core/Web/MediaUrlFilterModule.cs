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

        private void OnBeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;
            string path = ctx.Request.Url.LocalPath;

            if (path.StartsWith(_mediaUrlPrefix))
            {
                string rewrite;
                if (AppSettings.UseFolderPathsForMediaUrls)
                {
                    rewrite = _handlerUrlPrefix + "MediaArchive://" + path.Substring(_mediaUrlPrefix.Length);
                }
                else
                {
                    string guid = path.Substring(_mediaUrlPrefix.Length, 36);
                    rewrite = _handlerUrlPrefix + guid;                    
                }

                if (!String.IsNullOrEmpty(ctx.Request.Url.Query))
                {
                    rewrite = rewrite + "&" + ctx.Request.Url.Query.Remove(0, 1);
                }

                ctx.RewritePath(rewrite);
            }
        }

        private void OnPostMapRequestHandler(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            if (ctx.Handler is Page && !ctx.Request.Url.LocalPath.StartsWith(C1UrlUtils.AdminRootPath))
            {
                ctx.Response.Filter = new MediaUrlFilter(ctx.Response.Filter, ctx);
            }
        }

        void IHttpModule.Init(HttpApplication app)
        {
            app.BeginRequest += OnBeginRequest;
            app.PostMapRequestHandler += OnPostMapRequestHandler;
        }

        void IHttpModule.Dispose() { }
    }
}
