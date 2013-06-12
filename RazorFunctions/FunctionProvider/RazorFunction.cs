using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.WebPages;
using System.Xml;

using Composite.Core.Types;
using Composite.Core.Xml;
using Composite.Functions;

using CompositeC1Contrib.FunctionProvider;
using CompositeC1Contrib.Web;

namespace CompositeC1Contrib.RazorFunctions.FunctionProvider
{
    public class RazorFunction : FileBasedFunction<RazorFunction>
    {
        private static object _lock = new object();
        private const string _lockKey = "__razor_execute_lock__";

        public RazorFunction(string ns, string name, string description, IDictionary<string, FunctionParameterHolder> parameters, Type returnType, string virtualPath, FileBasedFunctionProvider<RazorFunction> provider)
            : base(ns, name, description, parameters, returnType, virtualPath, provider)
        {
        }

        public override object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            var webPage = WebPage.CreateInstanceFromVirtualPath(VirtualPath);

            foreach (var param in parameters.AllParameterNames)
            {
                var value = parameters.GetParameter(param);

                Parameters[param].SetValue(webPage, value);
            }

            var output = ExecuteRazorPage(webPage);

            if (ReturnType == typeof(XhtmlDocument))
            {
                XhtmlDocument returnDoc = null;

                try
                {
                    returnDoc = XhtmlDocument.Parse(output);
                }
                catch (ArgumentException)
                {
                    returnDoc = gracefulDocument(output);
                }
                catch (InvalidOperationException)
                {
                    returnDoc = gracefulDocument(output);
                }
                catch (XmlException)
                {
                    returnDoc = gracefulDocument(output);
                }

                ContentFilterFacade.FilterContent(returnDoc, Name);

                return returnDoc;
            }

            return ValueTypeConverter.Convert(output, ReturnType);
        }

        public static string ExecuteRazorPage(WebPageBase webPage)
        {
            HttpContextBase httpContext;
            object requestLock = null;

            var startPage = StartPage.GetStartPage(webPage, "_PageStart", new[] { "cshtml" });

            if (HttpContext.Current == null)
            {
                httpContext = new NoHttpRazorContext();
            }
            else
            {
                var currentContext = HttpContext.Current;

                httpContext = new HttpContextWrapper(currentContext);

                lock (_lock)
                {
                    requestLock = currentContext.Items[_lockKey];

                    if (requestLock == null)
                    {
                        requestLock = new object();

                        lock (currentContext.Items.SyncRoot)
                        {
                            currentContext.Items[_lockKey] = requestLock;
                        }
                    }
                }
            }

            var pageContext = new WebPageContext(httpContext, webPage, startPage);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                bool lockTaken = false;
                try
                {
                    if (requestLock != null)
                    {
                        Monitor.TryEnter(requestLock, ref lockTaken);
                    }

                    webPage.ExecutePageHierarchy(pageContext, writer);
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(requestLock);
                    }
                }
            }

            return sb.ToString().Trim();
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
