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
        private MvcPageTemplateDescriptor _template;
        private MvcRenderingContext _mvcContext;

        protected override void ExecuteCore()
        {
            base.ExecuteCore();

            _mvcContext.Dispose();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var pageUrlData = PageUrls.ParseUrl(HttpContext.Request.Path);
            if (pageUrlData == null)
            {
                return;
            }

            C1PageRoute.PageUrlData = pageUrlData;

            _mvcContext = new MvcRenderingContext(ControllerContext);

            var page = PageRenderer.CurrentPage;

            ViewData.Add("C1PreviewContent", HttpContext.Items["C1PreviewContent"]);

            _template = PageTemplateFacade.GetPageTemplate(page.TemplateId) as MvcPageTemplateDescriptor;
            if (_template == null)
            {
                throw new InvalidOperationException(String.Format("The pagetemplate '{0}' is not a valid mvc template", page.TemplateId));
            }

            var templateModel = Activator.CreateInstance(_template.TypeInfo.Item1);

            if (_template.PlaceholderDescriptions != null && _template.PlaceholderDescriptions.Any())
            {
                foreach (var placeholder in _template.PlaceholderDescriptions)
                {
                    var name = placeholder.Id;
                    var prop = _template.TypeInfo.Item2[name];
                    var content = GetPlaceholderContent(name);

                    BindPlaceholder(templateModel, prop, content);
                }
            }

            ViewData.Add("TemplateModel", templateModel);

            base.OnActionExecuting(filterContext);
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
            if (_mvcContext == null)
            {
                throw new InvalidOperationException("MvcContext has not been set, make sure the action matches a C1 page.");
            }

            ViewData.Model = model;

            if (String.IsNullOrEmpty(view))
            {
                view = _template.ViewName;
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

    public abstract class C1Controller<T> : C1Controller
    {
        public T TemplateModel
        {
            get { return (T)ViewData["TemplateModel"]; }
        }
    }
}
