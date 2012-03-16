using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.WebPages;
using System.Xml;

using Composite.Core.Types;
using Composite.Core.Xml;
using Composite.Functions;

using CompositeC1Contrib.FunctionProvider;

namespace CompositeC1Contrib.RazorFunctions.FunctionProvider
{
    public class RazorFunction : FileBasedFunction<RazorFunction>
    {
        public RazorFunction(string ns, string name, string description, IDictionary<string, FunctionParameterHolder> parameters, Type returnType, string virtualPath, FileBasedFunctionProvider<RazorFunction> provider)
            : base(ns, name, description, parameters, returnType, virtualPath, provider)
        {
        }

        public override object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            var webPage = WebPage.CreateInstanceFromVirtualPath(VirtualPath);

            HttpContextBase httpContext;

            if (HttpContext.Current == null)
            {
                httpContext = new NoHttpRazorContext();
            }
            else
            {
                httpContext = new HttpContextWrapper(HttpContext.Current);
            }

            var startPage = StartPage.GetStartPage(webPage, "_PageStart", new[] { "cshtml" });
            var pageContext = new WebPageContext(httpContext, webPage, startPage);

            foreach (var param in parameters.AllParameterNames)
            {
                var value = parameters.GetParameter(param);

                Parameters[param].SetValue(webPage, value);
            }

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                webPage.ExecutePageHierarchy(pageContext, writer);
            }

            string output = sb.ToString().Trim();

            if (ReturnType == typeof(XhtmlDocument))
            {
                try
                {
                    return XhtmlDocument.Parse(output);
                }
                catch (ArgumentException)
                {
                    return gracefulDocument(output);
                }
                catch (InvalidOperationException)
                {
                    return gracefulDocument(output);
                }
                catch (XmlException)
                {
                    return gracefulDocument(output);
                }
            }

            return ValueTypeConverter.Convert(output, ReturnType);
        }

        private static XhtmlDocument gracefulDocument(string content)
        {
            var s = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" xmlns:lang=\"http://www.composite.net/ns/localization/1.0\">" +
                    "<head />" +
                    "<body>" + content + "</body>" +
                    "</html>";

            return XhtmlDocument.Parse(s);
        }
    }
}
