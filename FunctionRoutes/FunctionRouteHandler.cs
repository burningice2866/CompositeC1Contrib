using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data;
using Composite.Functions;

namespace CompositeC1Contrib.FunctionRoutes
{
    class FunctionRouteHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            var functionName = (string)context.Request.RequestContext.RouteData.Values["function"];

            var dataCulture = GetCurrentDataCulture(context);

            using (var data = new DataConnection())
            {
                if (!data.Get<IFunctionRoute>().Any())
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return;
                }
            }

            IFunction function;
            if (!FunctionFacade.TryGetFunction(out function, functionName))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                return;
            }

            using (var dataScope = new DataScope(DataScopeIdentifier.Public, dataCulture))
            {
                var functionResult = FunctionFacade.Execute<object>(function, context.Request.QueryString);
                if (functionResult == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                var xhtmlDocument = functionResult as XhtmlDocument;
                if (xhtmlDocument != null)
                {
                    PageRenderer.ExecuteEmbeddedFunctions(xhtmlDocument.Root, new FunctionContextContainer());
                    PageRenderer.NormalizeXhtmlDocument(xhtmlDocument);

                    var xhtml = xhtmlDocument.ToString();

                    xhtml = PageUrlHelper.ChangeRenderingPageUrlsToPublic(functionResult.ToString());
                    xhtml = MediaUrlHelper.ChangeInternalMediaUrlsToPublic(xhtml);

                    context.Response.Write(xhtml);
                }
                else
                {
                    context.Response.Write(functionResult.ToString());

                    if (functionResult is XNode && function.ReturnType != typeof(XhtmlDocument))
                    {
                        context.Response.ContentType = "text/xml";
                    }
                }
            }
        }

        private CultureInfo GetCurrentDataCulture(HttpContext context)
        {
            var cultureScope = context.Request.QueryString["cultureScope"];
            if (String.IsNullOrEmpty(cultureScope) == false)
            {
                return new CultureInfo(cultureScope);
            }

            return DataLocalizationFacade.DefaultLocalizationCulture;
        }
    }
}
