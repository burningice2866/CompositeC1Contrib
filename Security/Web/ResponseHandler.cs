using System;
using System.Threading;
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

            return new Uri(loginPage + "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl.ToString()));
        }

        public RenderingResponseHandlerResult GetDataResponseHandling(DataEntityToken requestedItemEntityToken)
        {
            var result = new RenderingResponseHandlerResult();
            var ctx = HttpContext.Current;
            var isAuthenticated = Thread.CurrentPrincipal.Identity.IsAuthenticated;

            var page = requestedItemEntityToken.Data as IPage;
            if (page != null)
            {
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

                if (!result.EndRequest)
                {
                    if (!PermissionsFacade.HasAccess(page))
                    {
                        Uri loginUri;

                        if (isLoginPage)
                        {
                            loginUri = new Uri(ctx.Request.Url, "/");
                        }
                        else
                        {
                            loginUri = GetLoginUri();
                        }

                        EndResult(result, loginUri);
                    }
                }
            }

            if (isAuthenticated)
            {
                result.PreventPublicCaching = true;
            }

            return result;
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
            result.RedirectRequesterTo = uri;
        }
    }
}
