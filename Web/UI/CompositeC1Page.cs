using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Xml.Linq;

using Composite.C1Console.Security;
using Composite.Core;
using Composite.Core.Extensions;
using Composite.Core.Instrumentation;
using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.UI
{
    public class CompositeC1Page : Page
    {
        private static readonly string ProfilerXslPath = Composite.Core.WebClient.UrlUtils.PublicRootPath + "/Composite/Transformations/page_profiler.xslt";

        private IDisposable _dataScope;

        private bool _profilingEnabled = false;
        private IDisposable _pagePerfMeasuring;
        private IDisposable _pageEventsPageMeasuring;

        private PageUrl _url;
        private NameValueCollection _foreignQueryStringParameters;
        private string _cacheUrl = null;
        private bool _requestCompleted = false;

        public IPage Document
        {
            get { return PageRenderer.CurrentPage; }
            private set
            {
                if (PageRenderer.CurrentPage == null)
                {
                    PageRenderer.CurrentPage = value;
                }
            }
        }

        protected override void OnPreInit(EventArgs e)
        {
            var rq = RequestInfo.Current;

            if (rq.IsPreview)
            {
                Document = (IPage)Cache.Get(rq.PreviewKey + "_SelectedPage");
                _url = new PageUrl(PublicationScope.Unpublished, CultureInfo.CreateSpecificCulture(Document.CultureName), Document.Id);
                _dataScope = new DataScope(DataScopeIdentifier.FromPublicationScope(_url.PublicationScope), _url.Locale);
            }
            else
            {
                _profilingEnabled = UserValidationFacade.IsLoggedIn() && Request.Url.OriginalString.Contains("c1mode=perf");
                if (_profilingEnabled)
                {
                    Profiler.BeginProfiling();
                    _pagePerfMeasuring = Profiler.Measure("C1 Page");
                }

                _url = PageUrl.Parse(Context.Request.Url.OriginalString, out _foreignQueryStringParameters);
                _dataScope = new DataScope(DataScopeIdentifier.FromPublicationScope(_url.PublicationScope), _url.Locale);
                Document = PageManager.GetPageById(_url.PageId);

                _cacheUrl = Request.Url.PathAndQuery;
                RewritePath();
            }

            ValidateViewUnpublishedRequest();

            if (Document == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Page not found");
            }

            InitializeCulture();

            IPageTemplate template = null;
            using (var conn = new DataConnection())
            {
                template = conn.Get<IPageTemplate>().Single(t => t.Id == Document.TemplateId);
            }

            string masterFile = String.Format("~/Renderers/Masters/{0}.master", template.Title);
            if (HostingEnvironment.VirtualPathProvider.FileExists(masterFile))
            {
                AppRelativeVirtualPath = "~/";
                MasterPageFile = masterFile;
            }

            if (!rq.IsPreview)
            {
                _cacheUrl = Request.Url.PathAndQuery;

                var cachePolicy = Context.Response.Cache;
                cachePolicy.SetVaryByCustom("C1Page_ChangeDate");
                cachePolicy.SetExpires(DateTime.Now.AddSeconds(60));
                cachePolicy.VaryByParams["*"] = true;

                RewritePath();
            }

            base.OnPreInit(e);
        }

        protected override void InitializeCulture()
        {
            var doc = Document;
            if (doc != null)
            {
                this.Culture = this.UICulture = doc.CultureName;
            }

            base.InitializeCulture();
        }

        protected override void OnInit(EventArgs e)
        {
            var rq = RequestInfo.Current;

            if (_url == null || _url.PublicationScope != PublicationScope.Published || Request.IsSecureConnection)
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }

            if (!rq.IsPreview)
            {
                var responseHandling = RenderingResponseHandlerFacade.GetDataResponseHandling(PageRenderer.CurrentPage.GetDataEntityToken());
                if (responseHandling != null)
                {
                    if (responseHandling.PreventPublicCaching == true)
                    {
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    }

                    if (responseHandling.EndRequest || responseHandling.RedirectRequesterTo != null)
                    {
                        if (responseHandling.RedirectRequesterTo != null)
                        {
                            Response.Redirect(responseHandling.RedirectRequesterTo.AbsoluteUri, false);
                        }

                        Context.ApplicationInstance.CompleteRequest();
                        _requestCompleted = true;

                        return;
                    }
                }
            }

            var contents = rq.IsPreview ? (IEnumerable<IPagePlaceholderContent>)Cache.Get(rq.PreviewKey + "_SelectedContents") : PageManager.GetPlaceholderContent(PageRenderer.CurrentPage.Id);

            if (Master != null)
            {
                using (Profiler.Measure("Executing Page as Master"))
                {
                    var normalizeXhtmlDocument = typeof(PageRenderer).GetMethod("NormalizeXhtmlDocument", BindingFlags.Static | BindingFlags.NonPublic);
                    var resolveRelativePaths = typeof(PageRenderer).GetMethod("ResolveRelativePaths", BindingFlags.Static | BindingFlags.NonPublic);

                    foreach (var content in contents)
                    {
                        var doc = XElement.Parse(content.Content);
                        var context = PageRenderer.GetPageRenderFunctionContextContainer();

                        using (Profiler.Measure("Executing C1 functions"))
                        {
                            PageRenderer.ExecuteEmbeddedFunctions(doc, context);

                            var xDoc = new XhtmlDocument(doc);

                            normalizeXhtmlDocument.Invoke(null, new[] { xDoc });
                            resolveRelativePaths.Invoke(null, new[] { xDoc });
                        }

                        using (Profiler.Measure("ASP.NET controls: PageInit"))
                        {
                            var plc = FindControlRecursive(Master, content.PlaceHolderId);
                            if (plc != null)
                            {
                                var body = doc.Descendants().Single(el => el.Name.LocalName == "body");
                                var c = body.AsAspNetControl((IXElementToControlMapper)context.XEmbedableMapper);

                                plc.Controls.Add(c);
                            }

                            var head = doc.Descendants().SingleOrDefault(el => el.Name.LocalName == "head");
                            if (Header != null && head != null)
                            {
                                Header.Controls.Add(new LiteralControl(String.Concat(head.Elements())));
                            }
                        }
                    }
                }
            }
            else
            {
                Control renderedPage;
                using (Profiler.Measure("Executing C1 functions"))
                {
                    renderedPage = PageRenderer.Render(Document, contents);
                }

                if (rq.IsPreview)
                {
                    PageRenderer.DisableAspNetPostback(renderedPage);
                }

                using (Profiler.Measure("ASP.NET controls: PageInit"))
                {
                    Controls.Add(renderedPage);
                }
            }

            if (Form != null)
            {
                Form.Action = Request.RawUrl;
            }

            _pageEventsPageMeasuring = Profiler.Measure("ASP.NET controls: PageLoad, Event handling, PreRender");

            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (_requestCompleted)
            {
                return;
            }

            if (_pageEventsPageMeasuring != null)
            {
                _pageEventsPageMeasuring.Dispose();
            }

            var scriptManager = ScriptManager.GetCurrent(this);
            bool isUpdatePanelPostback = scriptManager != null && scriptManager.IsInAsyncPostBack;

            if (isUpdatePanelPostback == true)
            {
                base.Render(writer);
                return;
            }

            var markupBuilder = new StringBuilder();
            var sw = new StringWriter(markupBuilder);
            try
            {
                using (Profiler.Measure("ASP.NET controls: Render"))
                {
                    base.Render(new HtmlTextWriter(sw));
                }
            }
            catch (HttpException ex)
            {
                MethodInfo setStringMethod = typeof(HttpContext).Assembly /* System.Web */
                    .GetType("System.Web.SR")
                    .GetMethod("GetString", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);

                string multipleFormNotAllowedMessage = (string)setStringMethod.Invoke(null, new object[] { "Multiple_forms_not_allowed" });

                bool multipleAspFormTagsExists = ex.Message == multipleFormNotAllowedMessage;
                if (multipleAspFormTagsExists)
                {
                    throw new HttpException("Multiple <asp:form /> elements exists on this page. ASP.NET only support one form. To fix this, insert a <asp:form> ... </asp:form> section in your template that spans all controls.");
                }

                throw;
            }

            string xhtml;

            using (Profiler.Measure("Changing 'internal' page urls to 'public'"))
            {
                xhtml = PageUrlHelper.ChangeRenderingPageUrlsToPublic(markupBuilder.ToString());
            }

            try
            {
                using (Profiler.Measure("Formatting output XHTML with Composite.Core.Xml.XhtmlPrettifier"))
                {
                    xhtml = Composite.Core.Xml.XhtmlPrettifier.Prettify(xhtml);
                }
            }
            catch
            {
                Log.LogWarning("/Renderers/Page.aspx", "Failed to format output xhtml. Url: " + (_cacheUrl ?? String.Empty));
            }

            // Inserting perfomance profiling information
            if (_profilingEnabled)
            {
                _pagePerfMeasuring.Dispose();

                xhtml = BuildProfilerReport(Profiler.EndProfiling());

                Response.ContentType = "text/xml";
            }

            writer.Write(xhtml);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            if (_dataScope != null)
            {
                _dataScope.Dispose();
            }

            if (_requestCompleted)
            {
                return;
            }

            var rq = RequestInfo.Current;

            if (rq.IsPreview)
            {
                Cache.Remove(rq.PreviewKey + "_SelectedPage");
                Cache.Remove(rq.PreviewKey + "_SelectedPage");
            }

            // Rewrite path to what it was when this page was constructed. This ensure full page caching can work.
            if (_cacheUrl != null)
            {
                Context.RewritePath(_cacheUrl.Replace("%20", " "));
            }
        }

        public Control FindControlRecursive(Control current, string controlID)
        {
            if (current == null) throw new ArgumentNullException("current");
            if (controlID == null) throw new ArgumentNullException("controlID");

            if (current.ID == controlID)
            {
                return current;
            }

            foreach (Control c in current.Controls)
            {
                var t = FindControlRecursive(c, controlID);
                if (t != null) return t;
            }

            return null;
        }

        private void ValidateViewUnpublishedRequest()
        {
            if (_url != null
                && _url.PublicationScope != PublicationScope.Published
                && !UserValidationFacade.IsLoggedIn())
            {
                Response.Redirect(String.Format("{0}/Composite/Login.aspx?ReturnUrl={1}", Composite.Core.WebClient.UrlUtils.PublicRootPath, HttpUtility.UrlEncodeUnicode(Request.Url.OriginalString)), true);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void RewritePath()
        {
            var structuredUrl = _url.Build(PageUrlType.Public);
            if (structuredUrl == null)
            {
                return;
            }

            structuredUrl.AddQueryParameters(_foreignQueryStringParameters);

            string pathInfo = new UrlBuilder(_cacheUrl).PathInfo;
            Context.RewritePath(structuredUrl.FilePath, pathInfo, structuredUrl.QueryString);
        }

        private string BuildProfilerReport(Measurement measurement)
        {
            string xmlHeader = @"<?xml version=""1.0""?><?xml-stylesheet type=""text/xsl"" href=""{0}""?>".FormatWith(ProfilerXslPath);

            var reportXml = ProfilerReport.BuildReportXml(measurement);
            var url = new UrlBuilder(Context.Request.Url.ToString());
            url["c1mode"] = null;

            reportXml.Add(new XAttribute("description", "URL: " + url));

            return xmlHeader + reportXml.ToString();
        }
    }
}
