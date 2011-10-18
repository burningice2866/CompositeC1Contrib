using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.WebPages;
using System.Xml.Linq;

using Composite.C1Console.Security;
using Composite.Core.Xml;
using Composite.Functions;

namespace CompositeC1Contrib.RazorFunctions
{
    public class RazorFunction : IFunction
    {
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
            get { return Enumerable.Empty<ParameterProfile>(); }
        }

        public Type ReturnType
        {
            get { return typeof(XhtmlDocument); }
        }

        public RazorFunction(string ns, string name)
        {
            _ns = ns;
            _name = name;
        }

        public object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            var file = "~/App_Data/Razor";
            foreach (var part in Namespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                file = Path.Combine(file, part);
            }

            file = Path.Combine(file, Name + ".cshtml");

            var webPage = CompositeC1WebPage.CreateInstanceFromVirtualPath(file);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var httpContext = new HttpContextWrapper(HttpContext.Current);

                webPage.ExecutePageHierarchy(new WebPageContext(httpContext, webPage, null), writer);

                return new XhtmlDocument(XElement.Parse(sb.ToString()));
            }
        }
    }
}
