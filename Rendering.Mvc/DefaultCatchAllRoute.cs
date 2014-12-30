using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Composite.Core.PageTemplates;
using Composite.Core.Routing;
using Composite.Data;

using CompositeC1Contrib.Rendering.Mvc.Templates;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class DefaultCatchAllRoute : RouteBase
    {
        private readonly ControllerDescriptor _controller;

        public DefaultCatchAllRoute()
        {
            _controller = new ReflectedControllerDescriptor(typeof(DefaultCatchAllController));
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var path = httpContext.Request.Path;

            var pageUrlData = PageUrls.ParseUrl(path);
            if (pageUrlData == null)
            {
                return null;
            }

            using (new DataScope(pageUrlData.PublicationScope, pageUrlData.LocalizationScope))
            {
                var page = PageManager.GetPageById(pageUrlData.PageId);
                var templateIds = PageTemplateFacade.GetPageTemplates().OfType<MvcPageTemplateDescriptor>().Select(t => t.Id);

                if (!templateIds.Contains(page.TemplateId))
                {
                    return null;
                }

                var template = PageTemplateFacade.GetPageTemplate(page.TemplateId) as MvcPageTemplateDescriptor;
                if (template == null)
                {
                    return null;
                }
            }

            return new RouteData(this, new MvcRouteHandler())
            {
                Values =
                {
                    {"controller", _controller.ControllerName},
                    {"action", "Get"}
                }
            };
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return null;
        }
    }
}
