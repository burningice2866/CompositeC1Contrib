using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.WebPages;
using System.Xml;
using System.Xml.Linq;

using Composite.C1Console.Security;
using Composite.Core.Xml;
using Composite.Functions;

namespace CompositeC1Contrib.RazorFunctions
{
    public class RazorFunction : IFunction
    {
        string _relativeFilePath;
        IEnumerable<FunctionParameterHolder> _parameters;

        private string _ns;
        public string Namespace
        {
            get { return _ns; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return "Razor function"; }
        }

        public EntityToken EntityToken
        {
            get { return new RazorFunctionEntityToken(Name, String.Join(".", Namespace, Name)); }
        }

        public IEnumerable<ParameterProfile> ParameterProfiles
        {
            get
            {
                if (_parameters != null)
                {
                    foreach (var param in _parameters)
                    {
                        BaseValueProvider defaultValueProvider = new NoValueValueProvider();
                        WidgetFunctionProvider widgetProvider = null;
                        var label = param.Name;
                        var isRequired = true;
                        var helpText = String.Empty;

                        if (param.Attribute != null)
                        {
                            label = param.Attribute.Label;
                            helpText = param.Attribute.HelpText;

                            isRequired = !param.Attribute.HasDefaultValue;
                            if (!isRequired)
                            {
                                defaultValueProvider = new ConstantValueProvider(param.Attribute.DefaultValue);
                            }

                            if (!String.IsNullOrEmpty(param.Attribute.WidgetMarkup))
                            {
                                var xElement = XElement.Parse(param.Attribute.WidgetMarkup);

                                widgetProvider = new WidgetFunctionProvider(xElement);
                            }
                        }

                        if (widgetProvider == null)
                        {
                            widgetProvider = StandardWidgetFunctions.GetDefaultWidgetFunctionProviderByType(param.Type);
                        }

                        yield return new ParameterProfile(param.Name, param.Type, isRequired, defaultValueProvider, widgetProvider, label, new HelpDefinition(helpText));
                    }
                }
            }
        }

        public Type ReturnType
        {
            get { return typeof(XhtmlDocument); }
        }

        public RazorFunction(string ns, string name, IEnumerable<FunctionParameterHolder> parameters, string relativeFilePath)
        {
            _ns = ns;
            _name = name;
            _parameters = parameters;
            _relativeFilePath = relativeFilePath;
        }

        public object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            var webPage = WebPage.CreateInstanceFromVirtualPath(_relativeFilePath);

            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var pageContext = new WebPageContext(httpContext, webPage, null);

            foreach (var param in parameters.AllParameterNames)
            {
                var value = parameters.GetParameter(param);

                _parameters.Single(p => p.Name == param).SetValue(webPage, value);
            }

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                webPage.ExecutePageHierarchy(pageContext, writer);
            }

            try
            {
                return XhtmlDocument.Parse(sb.ToString());
            }
            catch (ArgumentException)
            {
                return gracefulDocument(sb.ToString());
            }
            catch (InvalidOperationException)
            {
                return gracefulDocument(sb.ToString());
            }
            catch (XmlException)
            {
                return gracefulDocument(sb.ToString());
            }
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
