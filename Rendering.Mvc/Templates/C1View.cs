using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.Instrumentation;
using Composite.Core.Localization;
using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Functions;

namespace CompositeC1Contrib.Rendering.Mvc.Templates
{
    public class C1View : ViewResult
    {
        private readonly MvcRenderingContext _mvcContext;

        public C1View(MvcRenderingContext mvcContext)
        {
            _mvcContext = mvcContext;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            string markup;

            using (TimerProfilerFacade.CreateTimerProfiler())
            {
                var page = PageRenderer.CurrentPage;
                var markupBuilder = new StringBuilder();
                var sw = new StringWriter(markupBuilder);
                var output = new HtmlTextWriter(sw);

                IView view;
                using (Profiler.Measure("Resolving view for template"))
                {
                    view = FindView(context).View;
                }

                var viewContext = new ViewContext(context, view, ViewData, TempData, output);

                view.Render(viewContext, output);

                markup = markupBuilder.ToString();
                var xml = XDocument.Parse(markup);

                var functionContext = PageRenderer.GetPageRenderFunctionContextContainer();

                functionContext = new FunctionContextContainer(functionContext, new Dictionary<string, object>
                {
                    {"viewContext", viewContext}
                });

                using (Profiler.Measure("Executing embedded functions"))
                {
                    PageRenderer.ExecuteEmbeddedFunctions(xml.Root, functionContext);
                }

                using (Profiler.Measure("Resolving pagefields"))
                {
                    PageRenderer.ResolvePageFields(xml, page);
                }

                var document = new XhtmlDocument(xml);

                using (Profiler.Measure("Normalizing html"))
                {
                    PageRenderer.NormalizeXhtmlDocument(document);
                }

                PageRenderer.ResolveRelativePaths(document);
                PageRenderer.AppendC1MetaTags(page, document);

                using (Profiler.Measure("Resolving localized strings"))
                {
                    LocalizationParser.Parse(document);
                }

                markup = document.ToString();

                using (Profiler.Measure("Changing 'internal' page urls to 'public'"))
                {
                    markup = PageUrlHelper.ChangeRenderingPageUrlsToPublic(markup);
                }

                using (Profiler.Measure("Changing 'internal' media urls to 'public'"))
                {
                    markup = MediaUrlHelper.ChangeInternalMediaUrlsToPublic(markup);
                }

                markup = _mvcContext.FormatXhtml(markup);
            }

            if (_mvcContext.ProfilingEnabled)
            {
                markup = _mvcContext.BuildProfilerReport();

                context.HttpContext.Response.ContentType = "text/xml";
            }

            context.HttpContext.Response.Write(markup);
        }
    }
}
