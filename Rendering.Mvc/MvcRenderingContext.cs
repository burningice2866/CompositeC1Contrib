using System;
using System.Collections.Generic;
using System.Net;
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
        private readonly HttpContextBase _context;

        private readonly string _cachedUrl;
        private readonly IDisposable _dataScope;
        private readonly IDisposable _pagePerfMeasuring;
        private static int _prettifyErrorCount;
        private static readonly List<string> PrettifyErrorUrls = new List<string>();
        private static readonly string ProfilerXslPath = (UrlUtils.AdminRootPath + "/Transformations/page_profiler.xslt");

        public bool CachingDisabled { get; private set; }
        public IPage Page { get; private set; }
        public bool ProfilingEnabled { get; private set; }

        public MvcRenderingContext(ControllerContext context)
        {
            _context = context.HttpContext;
            var request = _context.Request;

            if (PageRenderer.CurrentPage == null)
            {
                var response = _context.Response;

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

                Page = (C1PageRoute.PageUrlData ?? PageUrls.UrlProvider.ParseInternalUrl(request.Url.OriginalString)).GetPage();
                _cachedUrl = request.Url.PathAndQuery;

                ValidateViewUnpublishedRequest();

                if (Page == null)
                {
                    throw new HttpException((int)HttpStatusCode.NotFound, "Page not found - either this page has not been published yet or it has been deleted.");
                }

                if ((Page.DataSourceId.PublicationScope != PublicationScope.Published) || request.IsSecureConnection)
                {
                    CachingDisabled = true;

                    response.Cache.SetCacheability(HttpCacheability.NoCache);
                }

                PageRenderer.CurrentPage = Page;
                PageRenderer.RenderingReason = new UrlSpace(_context).ForceRelativeUrls ? RenderingReason.C1ConsoleBrowserPageView : RenderingReason.PageView;

                _dataScope = new DataScope(Page.DataSourceId.PublicationScope, Page.DataSourceId.LocaleScope);

            }
            else
            {
                Page = PageRenderer.CurrentPage;
            }

            C1PageRoute.PageUrlData = new PageUrlData(Page);
        }

        public string BuildProfilerReport()
        {
            _pagePerfMeasuring.Dispose();

            var measurement = Profiler.EndProfiling();
            var str = String.Format("<?xml version=\"1.0\"?>\r\n" +
                      "<?xml-stylesheet type=\"text/xsl\" href=\"{0}\"?>", ProfilerXslPath);

            var element = ProfilerReport.BuildReportXml(measurement);

            var builder = new UrlBuilder(_context.Request.Url.ToString());
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
                if (_prettifyErrorCount >= 3)
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

                    if (_prettifyErrorCount == 3)
                    {
                        Log.LogInformation("/Renderers/Page.aspx", "{0} xhtml format errors logged since startup. No more will be logged untill next startup.", new object[] { 3 });
                    }

                    return xhtml;
                }
            }

            return xhtml;
        }

        private static string GetLoginRedirectUrl(string url)
        {
            return (UrlUtils.PublicRootPath + "/Composite/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(url, Encoding.UTF8));
        }

        private void ValidateViewUnpublishedRequest()
        {
            var isRelative = _context.Request.Url.OriginalString.Contains("/c1mode(relative)");

            if ((((Page == null) || (Page.DataSourceId.PublicationScope == PublicationScope.Published)) && !isRelative) ||
                UserValidationFacade.IsLoggedIn())
            {
                return;
            }

            var loginRedirectUrl = GetLoginRedirectUrl(_context.Request.Url.OriginalString);
            _context.Response.Redirect(loginRedirectUrl, true);
        }
    }
}
