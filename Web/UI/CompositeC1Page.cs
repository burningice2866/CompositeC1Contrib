using System;
using System.Collections.Generic;
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
using Composite.Core.Instrumentation;
using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.Types;
using System.Collections.Specialized;

namespace CompositeC1Contrib.Web.UI
{
    public class CompositeC1Page : Page
    {
        private static readonly string ProfilerXslPath = HostingEnvironment.MapPath("~/Composite/Transformations/page_profiler.xslt");

        private IDisposable _dataScope;

        private bool _profilingEnabled = false;
        private IDisposable _pagePerfMeasuring;
        private IDisposable _pageEventsPageMeasuring;

        private NameValueCollection _foreignQueryStringParameters;
        private PageUrl _url;
        private string _cacheUrl = null;
        private bool _requestCompleted = false;

        public IPage Document
        {
            get { return PageRenderer.CurrentPage; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            if (RequestInfo.Current.IsPreview)
            {
                PageRenderer.CurrentPage = (IPage)Context.Items["SelectedPage"];
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
                if (_url != null)
                {
                    if (_url.PublicationScope != PublicationScope.Published)
                    {
                        if (!UserValidationFacade.IsLoggedIn())
                        {
                            Response.Redirect(String.Format("/Composite/Login.aspx?ReturnUrl={0}", HttpUtility.UrlEncodeUnicode(Request.Url.OriginalString)), true);
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }

                    _dataScope = new DataScope(DataScopeIdentifier.FromPublicationScope(_url.PublicationScope), _url.Locale); // IDisposable, Disposed in OnUnload

                    PageRenderer.CurrentPage = PageManager.GetPageById(_url.PageId);
                }
            }

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

            if (!RequestInfo.Current.IsPreview)
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
            if (!RequestInfo.Current.IsPreview)
            {
                if (_url.PublicationScope != PublicationScope.Published)
                {
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                }

                var responseHandling = RenderingResponseHandlerFacade.GetDataResponseHandling(Document.GetDataEntityToken());

                if ((responseHandling != null) && (responseHandling.PreventPublicCaching == true))
                {
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                }

                if ((responseHandling != null) && (responseHandling.EndRequest || responseHandling.RedirectRequesterTo != null))
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

            var contents = RequestInfo.Current.IsPreview ? (IEnumerable<IPagePlaceholderContent>)Context.Items["SelectedContents"] : PageManager.GetPlaceholderContent(Document.Id);

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

                if (RequestInfo.Current.IsPreview)
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
            if (RequestInfo.Current.IsPreview)
            {
                base.Render(writer);
            }
            else
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
                    var setStringMethod = typeof(HttpContext).Assembly /* System.Web */
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
                    throw;
                }

                if (_profilingEnabled)
                {
                    _pagePerfMeasuring.Dispose();

                    xhtml = BuildProfilerReport(Profiler.EndProfiling());

                    Response.ContentType = "text/xml";
                }

                writer.Write(xhtml);
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            if (_dataScope != null)
            {
                _dataScope.Dispose();
            }

            if (_requestCompleted)
            {
                return;
            }

            if (_cacheUrl != null)
            {
                Context.RewritePath(_cacheUrl.Replace("%20", " "));
            }

            base.OnUnload(e);
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
            string xmlHeader = String.Format(@"<?xml version=""1.0""?>
                             <?xml-stylesheet type=""text/xsl"" href=""{0}""?>", ProfilerXslPath);

            var reportXml = ProfilerReport.BuildReportXml(measurement);
            var url = new UrlBuilder(Context.Request.Url.ToString());
            url["c1mode"] = null;

            reportXml.Add(new XAttribute("description", "URL: " + url));

            return xmlHeader + reportXml.ToString();
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
    }
}
