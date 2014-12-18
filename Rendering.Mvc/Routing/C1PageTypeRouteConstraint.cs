using System;
using System.Web;
using System.Web.Routing;

using Composite.Core.Routing;
using Composite.Data;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1PageTypeRouteConstraint : IRouteConstraint
    {
        private readonly Guid _typeId;

        public C1PageTypeRouteConstraint(Guid typeId)
        {
            _typeId = typeId;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var path = httpContext.Request.Path;

            var pageUrlData = PageUrls.ParseUrl(path);
            if (pageUrlData == null)
            {
                return false;
            }

            using (new DataScope(pageUrlData.PublicationScope, pageUrlData.LocalizationScope))
            {
                var page = PageManager.GetPageById(pageUrlData.PageId);

                return page.PageTypeId == _typeId;
            }
        }
    }
}
