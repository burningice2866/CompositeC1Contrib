using System;
using System.Web.Mvc;

using Composite.Core.PageTemplates;
using Composite.Core.Routing;
using Composite.Core.Routing.Pages;
using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public abstract class C1Controller : Controller
    {
        private MvcRenderingContext _mvcContext;

        protected override void ExecuteCore()
        {
            base.ExecuteCore();

            _mvcContext.Dispose();
        }

        public C1View C1View()
        {
            return C1View(null);
        }

        public C1View C1View(string view)
        {
            return C1View(view, null);
        }

        public C1View C1View(string view, object model)
        {
            var pageUrlData = PageUrls.ParseUrl(HttpContext.Request.Path);

            C1PageRoute.PageUrlData = pageUrlData;

            _mvcContext = MvcRenderingContext.InitializeFromContext(ControllerContext);

            var page = PageRenderer.CurrentPage;

            ViewData.Model = model;

            var template = PageTemplateFacade.GetPageTemplate(page.TemplateId) as MvcPageTemplateDescriptor;
            if (template == null)
            {
                throw new InvalidOperationException(String.Format("The pagetemplate '{0}' is not a valid mvc template", page.TemplateId));
            }

            if (String.IsNullOrEmpty(view))
            {
                view = template.ViewName;
            }

            return new C1View(_mvcContext)
            {
                ViewName = view,
                ViewData = ViewData,
                ViewEngineCollection = ViewEngineCollection
            }; ;
        }
    }
}
