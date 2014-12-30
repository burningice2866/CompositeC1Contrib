using System;
using System.IO;
using System.Web;
using System.Web.Routing;
using System.Web.UI;

using Composite.Core.PageTemplates;
using Composite.Core.Routing;

namespace CompositeC1Contrib.Rendering.Mvc.Templates
{
    public class MvcPageRenderer : IPageRenderer
    {
        private Page _renderTarget;
        private StringWriter _writer;

        public void AttachToPage(Page renderTarget, PageContentToRender contentToRender)
        {
            _renderTarget = renderTarget;
            _writer = new StringWriter();

            var pageUrl = PageUrls.BuildUrl(contentToRender.Page);
            var pageUri = new Uri(renderTarget.Request.Url, pageUrl);

            var httpRequest = new HttpRequest(null, pageUri.OriginalString, "");
            var httpResponse = new HttpResponse(_writer);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            if (routeData == null || routeData.RouteHandler == null)
            {
                renderTarget.Init += Renderer_NoRoute;
            }
            else
            {
                var requestContext = new RequestContext(new HttpContextWrapper(httpContext), routeData);
                var handler = routeData.RouteHandler.GetHttpHandler(requestContext);
                if (handler == null)
                {
                    renderTarget.Init += Renderer_NoRoute;
                }
                else
                {
                    httpContext.Items.Add("C1PreviewContent", contentToRender.Contents);

                    handler.ProcessRequest(httpContext);

                    renderTarget.Init += Renderer;
                }
            }
        }

        private void Renderer(object sender, EventArgs e)
        {
            var pageOutput = _writer.ToString();
            _renderTarget.Controls.Add(new LiteralControl(pageOutput));

            _writer.Dispose();
        }

        private void Renderer_NoRoute(object sender, EventArgs e)
        {
            _renderTarget.Controls.Add(new LiteralControl("No route for this request was found :("));

            _writer.Dispose();
        }
    }
}
