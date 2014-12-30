using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using Composite;
using Composite.Core.PageTemplates;
using Composite.Core.Routing;
using Composite.Core.Routing.Pages;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Rendering.Mvc.Templates;

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

        public C1View C1View(object model)
        {
            return C1View(null, model);
        }

        public C1View C1View(string view, object model)
        {
            var pageUrlData = PageUrls.ParseUrl(HttpContext.Request.Path);

            C1PageRoute.PageUrlData = pageUrlData;

            _mvcContext = new MvcRenderingContext(ControllerContext);

            var page = PageRenderer.CurrentPage;

            ViewData.Model = model;
            ViewData.Add("C1PreviewContent", HttpContext.Items["C1PreviewContent"]);

            var template = PageTemplateFacade.GetPageTemplate(page.TemplateId) as MvcPageTemplateDescriptor;
            if (template == null)
            {
                throw new InvalidOperationException(String.Format("The pagetemplate '{0}' is not a valid mvc template", page.TemplateId));
            }

            if (template.PlaceholderDescriptions != null && template.PlaceholderDescriptions.Any())
            {
                var templateModel = Activator.CreateInstance(template.TypeInfo.Item1);

                foreach (var placeholder in template.PlaceholderDescriptions)
                {
                    var name = placeholder.Id;
                    var prop = template.TypeInfo.Item2[name];
                    var content = GetPlaceholderContent(name);

                    BindPlaceholder(templateModel, prop, content);
                }

                ViewData.Add("TemplateModel", templateModel);
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
            };
        }

        private IPagePlaceholderContent GetPlaceholderContent(string id)
        {
            var previewContent = ViewData["C1PreviewContent"];
            if (previewContent != null)
            {
                var placeholderContent = ((IEnumerable<IPagePlaceholderContent>)previewContent).SingleOrDefault(p => p.PlaceHolderId == id);

                return placeholderContent;
            }
            else
            {
                var page = PageRenderer.CurrentPage;
                var placeholderContent = PageManager.GetPlaceholderContent(page.Id).SingleOrDefault(p => p.PlaceHolderId == id);

                return placeholderContent;
            }
        }

        public static void BindPlaceholder(object templateModel, PropertyInfo property, IPagePlaceholderContent content)
        {
            var rootDocument = PageRenderer.ParsePlaceholderContent(content);

            PageRenderer.ResolveRelativePaths(rootDocument);

            if (property.ReflectedType != null && !property.ReflectedType.IsInstanceOfType(templateModel))
            {
                var name = property.Name;
                property = templateModel.GetType().GetProperty(property.Name);

                Verify.IsNotNull(property, "Failed to find placeholder property '{0}'", new object[] { name });
            }

            property.SetValue(templateModel, rootDocument);
        }
    }
}
