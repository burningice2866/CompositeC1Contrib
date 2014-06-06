using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Composite.AspNet;
using Composite.C1Console.Security;
using Composite.Core.Routing;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.Mvc
{
    public class ContentRoute : RouteBase
    {
        public override VirtualPathData GetVirtualPath(RequestContext ctx, RouteValueDictionary values)
        {
            return null;
        }

        public override RouteData GetRouteData(HttpContextBase ctx)
        {
            var scope = ctx.Request.QueryString["dataScope"] == "administrated" ? PublicationScope.Unpublished : PublicationScope.Published;

            if (scope != PublicationScope.Published && !UserValidationFacade.IsLoggedIn())
            {
                string url = String.Format("{0}/Composite/Login.aspx?ReturnUrl={1}", Composite.Core.WebClient.UrlUtils.PublicRootPath, HttpUtility.UrlEncodeUnicode(ctx.Request.Url.OriginalString));
                ctx.Response.Redirect(url, true);
                ctx.ApplicationInstance.CompleteRequest();
            }

            IPage page = null;
            DataScope dataScope = null;

            var node = (CompositeC1SiteMapNode)SiteMap.Provider.FindSiteMapNode(ctx.Request.RawUrl);
            if (node != null)
            {
                dataScope = new DataScope(DataScopeIdentifier.FromPublicationScope(scope), node.Culture);
                page = PageManager.GetPageById(node.PageNode.Id);
            }

            if (page == null)
            {
                NameValueCollection qs;

                var pageUrlData = PageUrls.ParseUrl(ctx.Request.RawUrl);
                if (pageUrlData != null)
                {
                    dataScope = new DataScope(DataScopeIdentifier.FromPublicationScope(pageUrlData.PublicationScope), pageUrlData.LocalizationScope);
                    page = PageManager.GetPageById(pageUrlData.PageId);
                }
            }

            if (page == null)
            {
                return new RouteData(this, new StopRoutingHandler());
            }

            using (var data = new DataConnection())
            {
                var pageType = data.Get<IPageType>().Single(p => p.Id == page.PageTypeId);
                var routeData = new RouteData(this, new MvcRouteHandler());

                routeData.DataTokens.Add("ID", page);
                routeData.Values["controller"] = pageType.Name;
                routeData.Values["action"] = "Index";
                routeData.Values["ID"] = page.Id;
                routeData.Values["dataScope"] = dataScope;

                return routeData;
            }
        }
    }
}
