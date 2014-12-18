using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

using Composite.C1Console.Security;
using Composite.Core;
using Composite.Core.Extensions;
using Composite.Core.Instrumentation;
using Composite.Core.Routing;
using Composite.Core.Routing.Pages;
using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class MvcRenderingContext : IDisposable
    {
        private string _cachedUrl;
        private IDisposable _dataScope;
        private IDisposable _pagePerfMeasuring;
        private static int _prettifyErrorCount;
        private static readonly List<string> PrettifyErrorUrls = new List<string>();
        private string _previewKey;
        private static readonly string ProfilerXslPath = (UrlUtils.AdminRootPath + "/Transformations/page_profiler.xslt");

        public bool CachingDisabled { get; private set; }

        public IPage Page { get; private set; }

        public bool PreviewMode { get; private set; }

        public bool ProfilingEnabled { get; private set; }

        public static MvcRenderingContext InitializeFromContext(ControllerContext context)
        {
            var renderingContext = new MvcRenderingContext();

            renderingContext.InitializeFromContextInternal(context);

            return renderingContext;
        }

        public string BuildProfilerReport()
        {
            _pagePerfMeasuring.Dispose();

            var measurement = Profiler.EndProfiling();
            var str = "<?xml version=\"1.0\"?>\r\n                             <?xml-stylesheet type=\"text/xsl\" href=\"{0}\"?>".FormatWith(new object[] { ProfilerXslPath });
            
            var element = ProfilerReport.BuildReportXml(measurement);
            
            var builder = new UrlBuilder(HttpContext.Current.Request.Url.ToString());
            builder["c1mode"] = null;

            element.Add(new XAttribute("url", builder));
            
            return (str + element);
        }

        public void Dispose()
        {
            if (_dataScope != null)
            {
                _dataScope.Dispose();
            }

            if (PreviewMode)
            {
                var cache = HttpRuntime.Cache;

                cache.Remove(_previewKey + "_SelectedPage");
                cache.Remove(_previewKey + "_SelectedContents");
            }
        }

        public string FormatXhtml(string xhtml)
        {
            try
            {
                using (Profiler.Measure("Formatting output XHTML with Composite.Core.Xml.XhtmlPrettifier"))
                {
                    xhtml = XhtmlPrettifier.Prettify(xhtml);
                }
            }
            catch
            {
                if (PreviewMode || (_prettifyErrorCount >= 3))
                {
                    return xhtml;
                }

                lock (PrettifyErrorUrls)
                {
                    if (PrettifyErrorUrls.Contains(_cachedUrl) || (_prettifyErrorCount >= 3))
                    {
                        return xhtml;
                    }

                    PrettifyErrorUrls.Add(_cachedUrl);
                    _prettifyErrorCount++;

                    Log.LogWarning("/Renderers/Page.aspx", "Failed to format output xhtml in a pretty way - your page output is likely not strict xml. Url: " + (HttpUtility.UrlDecode(_cachedUrl) ?? "undefined"));
                    
                    if (3 == _prettifyErrorCount)
                    {
                        Log.LogInformation("/Renderers/Page.aspx", "{0} xhtml format errors logged since startup. No more will be logged untill next startup.", new object[] { 3 });
                    }

                    return xhtml;
                }
            }

            return xhtml;
        }

        private void InitializeFromContextInternal(ControllerContext context)
        {
            var current = context.HttpContext;
            var request = context.RequestContext.HttpContext.Request;
            var response = context.RequestContext.HttpContext.Response;

            ProfilingEnabled = request.Url.OriginalString.Contains("c1mode=perf");
            if (ProfilingEnabled)
            {
                if (!UserValidationFacade.IsLoggedIn())
                {
                    response.Write("You must be logged into <a href=\"" + GetLoginRedirectUrl(request.RawUrl) + "\">C1 console</a> to have the performance view enabled");
                    response.End();

                    return;
                }

                Profiler.BeginProfiling();

                _pagePerfMeasuring = Profiler.Measure("C1 Page");
            }

            _previewKey = request.QueryString["previewKey"];

            PreviewMode = !_previewKey.IsNullOrEmpty();
            
            if (PreviewMode)
            {
                Page = (IPage)HttpRuntime.Cache.Get(_previewKey + "_SelectedPage");

                C1PageRoute.PageUrlData = new PageUrlData(Page);

                PageRenderer.RenderingReason = (RenderingReason)HttpRuntime.Cache.Get(_previewKey + "_RenderingReason");
            }
            else
            {
                Page = (C1PageRoute.PageUrlData ?? PageUrls.UrlProvider.ParseInternalUrl(request.Url.OriginalString)).GetPage();
                _cachedUrl = request.Url.PathAndQuery;

                PageRenderer.RenderingReason = new UrlSpace(current).ForceRelativeUrls ? RenderingReason.C1ConsoleBrowserPageView : RenderingReason.PageView;
            }

            ValidateViewUnpublishedRequest(current);

            if (Page == null)
            {
                throw new HttpException(0x194, "Page not found - either this page has not been published yet or it has been deleted.");
            }

            if ((Page.DataSourceId.PublicationScope != PublicationScope.Published) || request.IsSecureConnection)
            {
                response.Cache.SetCacheability(HttpCacheability.NoCache);
                CachingDisabled = true;
            }

            PageRenderer.CurrentPage = Page;

            _dataScope = new DataScope(Page.DataSourceId.PublicationScope, Page.DataSourceId.LocaleScope);
        }

        private static string GetLoginRedirectUrl(string url)
        {
            return (UrlUtils.PublicRootPath + "/Composite/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(url, Encoding.UTF8));
        }

        private void ValidateViewUnpublishedRequest(HttpContextBase httpContext)
        {
            const string urlMarkerRelativeUrl = "/c1mode(relative)";
            
            var flag = httpContext.Request.Url.OriginalString.Contains(urlMarkerRelativeUrl);
            if ((((Page == null) || (Page.DataSourceId.PublicationScope == PublicationScope.Published)) && !flag) ||
                UserValidationFacade.IsLoggedIn())
            {
                return;
            }

            var loginRedirectUrl = GetLoginRedirectUrl(httpContext.Request.Url.OriginalString);
            httpContext.Response.Redirect(loginRedirectUrl, true);
        }
    }
}
