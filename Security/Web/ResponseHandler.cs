using System;
using System.Web;
using System.Web.Security;

using Composite.Core.WebClient.Renderings;
using Composite.Core.WebClient.Renderings.Plugins.RenderingResponseHandler;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Security.Security;

namespace CompositeC1Contrib.Security.Web
{
    public class ResponseHandler : IDataRenderingResponseHandler
    {
        public static SiteMapNode LoginSiteMapNode
        {
            get
            {
                var loginPage = FormsAuthentication.LoginUrl;
                if (loginPage.StartsWith("/"))
                {
                    loginPage = loginPage.Remove(0, 1);
                }

                return SiteMap.Provider.FindSiteMapNodeFromKey(loginPage);
            }
        }

        public static Uri GetLoginUri()
        {
            var ctx = HttpContext.Current;
            var returnUrl = EnsureHttps(ctx.Request.Url).AbsolutePath;
            var loginPage = EnsureHttps(new Uri(ctx.Request.Url, LoginSiteMapNode.Url));

            return new Uri(loginPage + "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl));
        }

        public RenderingResponseHandlerResult GetDataResponseHandling(DataEntityToken requestedItemEntityToken)
        {
            var ctx = HttpContext.Current;
            var result = new RenderingResponseHandlerResult();

            var page = requestedItemEntityToken.Data as IPage;
            if (page != null)
            {
                HandlePageRequest(result, page);
            }

            var media = requestedItemEntityToken.Data as IMediaFile;
            if (media != null)
            {
                HandleMediaRequest(result, media);
            }

            if (ctx.User.Identity.IsAuthenticated)
            {
                result.PreventPublicCaching = true;
            }

            return result;
        }

        private static void HandlePageRequest(RenderingResponseHandlerResult result, IPage page)
        {
            var ctx = HttpContext.Current;
            var isSecureConnection = ctx.Request.IsSecureConnection;
            var isLoginPage = LoginSiteMapNode.Key == page.Id.ToString();

            if (isLoginPage)
            {
                if (FormsAuthentication.RequireSSL && !isSecureConnection)
                {
                    EndResult(result, EnsureHttps(ctx.Request.Url));
                }

                if (ctx.Request.QueryString["cmd"] == "logoff")
                {
                    FormsAuthentication.SignOut();
                    ctx.Session.Clear();

                    var uri = MakeNormal(new Uri(ctx.Request.Url, "/"));

                    EndResult(result, uri);
                }
            }

            if (result.EndRequest || PermissionsFacade.HasAccess(page))
            {
                return;
            }

            var loginUri = isLoginPage ? new Uri(ctx.Request.Url, "/") : GetLoginUri();

            EndResult(result, loginUri);
        }

        private static void HandleMediaRequest(RenderingResponseHandlerResult result, IMediaFile media)
        {
            if (PermissionsFacade.HasAccess(media))
            {
                return;
            }

            EndResult(result, null);
        }

        private static Uri MakeNormal(Uri uri)
        {
            var uriBuilder = new UriBuilder(uri)
            {
                Scheme = "http",
                Port = 80
            };

            return uriBuilder.Uri;
        }

        private static Uri EnsureHttps(Uri uri)
        {
            if (!FormsAuthentication.RequireSSL)
            {
                return uri;
            }

            var uriBuilder = new UriBuilder(uri)
            {
                Scheme = "https",
                Port = 443
            };

            return uriBuilder.Uri;
        }

        private static void EndResult(RenderingResponseHandlerResult result, Uri uri)
        {
            result.EndRequest = true;
            result.PreventPublicCaching = true;

            if (uri != null)
            {
                result.RedirectRequesterTo = uri;
            }
        }
    }
}
