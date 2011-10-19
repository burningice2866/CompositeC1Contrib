using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.WebPages;
using System.Xml;

using Composite.C1Console.Security;
using Composite.Core.Xml;
using Composite.Functions;

namespace CompositeC1Contrib.RazorFunctions
{
    public class RazorFunction : IFunction
    {
        string _relativeFilePath;
        ParameterInfo[] _parameters;

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
            get { return new FunctionsProviderEntityToken(Name, String.Join(".", Namespace, Name)); }
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

                        var attributes = param.GetCustomAttributes(typeof(FunctionParameterAttribute), false);
                        if (attributes != null && attributes.Length > 0)
                        {
                            var attr = (FunctionParameterAttribute)attributes[0];

                            var isRequired = !attr.HasDefaultValue;

                            if (!isRequired)
                            {
                                defaultValueProvider = new ConstantValueProvider(attr.DefaultValue);
                            }

                            yield return new ParameterProfile(param.Name, param.ParameterType, isRequired, defaultValueProvider, null, attr.Label, new HelpDefinition(attr.HelpText));
                        }
                        else
                        {
                            yield return new ParameterProfile(param.Name, param.ParameterType, true, defaultValueProvider, null, param.Name, new HelpDefinition(param.Name));
                        }
                    }
                }
            }
        }

        public Type ReturnType
        {
            get { return typeof(XhtmlDocument); }
        }

        public RazorFunction(string ns, string name, ParameterInfo[] parameters, string relativeFilePath)
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
                pageContext.PageData[param] = parameters.GetParameter(param);
            }

            string output = String.Empty;

            var mainMethod = webPage.GetType().GetMembers().SingleOrDefault(m => m.Name == "main") as MethodInfo;
            if (mainMethod != null)
            {
                var helper = (HelperResult)mainMethod.Invoke(webPage, parameters.AllParameterNames.Select(n => parameters.GetParameter(n)).ToArray());

                output = helper.ToHtmlString();
            }
            else
            {
                var sb = new StringBuilder();
                using (var writer = new StringWriter(sb))
                {
                    webPage.ExecutePageHierarchy(pageContext, writer);

                    output = sb.ToString();
                }
            }

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
