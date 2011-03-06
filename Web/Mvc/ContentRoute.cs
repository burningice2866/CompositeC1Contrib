using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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
            var node = CompositeC1SiteMapProvider.ResolveNodeFromUrl(ctx.Request.Url);
            if (node != null)
            {
                using (var data = new DataConnection(node.Culture))
                {
                    var page = data.Get<IPage>().Single(p => p.Id == node.PageNode.Id);
                    var pageType = data.Get<IPageType>().Single(p => p.Id == page.PageTypeId);
                    var routeData = new RouteData(this, new MvcRouteHandler());

                    routeData.DataTokens.Add("ID", page);
                    routeData.Values["controller"] = pageType.Name;
                    routeData.Values["action"] = "Index";
                    routeData.Values["ID"] = page.Id;

                    return routeData;
                }
            }

            return new RouteData(this, new StopRoutingHandler());
        }
    }
}
