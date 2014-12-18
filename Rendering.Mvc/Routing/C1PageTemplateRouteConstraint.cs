using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

using Composite.Core.PageTemplates;
using Composite.Core.Routing;
using Composite.Data;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1PageTemplateRouteConstraint : IRouteConstraint
    {
        private readonly HashSet<Guid> _templateIds;

        public C1PageTemplateRouteConstraint(IEnumerable<Guid> templateIds)
        {
            _templateIds = new HashSet<Guid>(templateIds);
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

                if (!_templateIds.Contains(page.TemplateId))
                {
                    return false;
                }

                var template = PageTemplateFacade.GetPageTemplate(page.TemplateId) as MvcPageTemplateDescriptor;

                return template != null;
            }
        }
    }
}
