using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.WebPages;
using System.Xml;
using System.Xml.Linq;

using Composite.C1Console.Security;
using Composite.Core.Types;
using Composite.Core.Xml;
using Composite.Functions;

using CompositeC1Contrib.RazorFunctions.Security;

namespace CompositeC1Contrib.RazorFunctions.FunctionProvider
{
    public class RazorFunction : IFunction
    {
        string _relativeFilePath;
        IDictionary<string, FunctionParameterHolder> _parameters;

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

        private Type _returnType;
        public Type ReturnType
        {
            get { return _returnType; }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
        }

        public EntityToken EntityToken
        {
            get { return new RazorFunctionEntityToken(typeof(RazorFunctionProvider).Name, String.Join(".", Namespace, Name)); }
        }

        public IEnumerable<ParameterProfile> ParameterProfiles
        {
            get
            {
                if (_parameters != null)
                {
                    foreach (var param in _parameters.Values)
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

        public RazorFunction(string ns, string name, string description, IDictionary<string, FunctionParameterHolder> parameters, Type returnType, string relativeFilePath)
        {
            _ns = ns;
            _name = name;
            _description = description;
            _parameters = parameters;
            _returnType = returnType;
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

                _parameters[param].SetValue(webPage, value);
            }

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                webPage.ExecutePageHierarchy(pageContext, writer);
            }

            string output = sb.ToString().Trim();

            if (_returnType == typeof(XhtmlDocument))
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

            return ValueTypeConverter.Convert(output, _returnType);
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
