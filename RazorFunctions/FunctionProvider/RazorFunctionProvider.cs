using System;
using System.Web.WebPages;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.Core.Xml;

using CompositeC1Contrib.FunctionProvider;

namespace CompositeC1Contrib.RazorFunctions.FunctionProvider
{
    [ConfigurationElementType(typeof(RazorFunctionProviderData))]
    public class RazorFunctionProvider : FileBasedFunctionProvider<RazorFunction>
    {
        protected override string FileExtension
        {
            get { return "cshtml"; }
        }

        protected override Type BaseType
        {
            get { return typeof(CompositeC1WebPage); }
        }

        public RazorFunctionProvider(string name, string folder) : base(name, folder) { }

        protected override Type GetReturnType(object obj)
        {
            return typeof(XhtmlDocument);
        }

        protected override object InstantiateFile(string virtualPath)
        {
            return WebPage.CreateInstanceFromVirtualPath(virtualPath);
        }

        protected override bool HandleChange(string path)
        {
            return path.EndsWith(".cshtml");
        }
    }
}
