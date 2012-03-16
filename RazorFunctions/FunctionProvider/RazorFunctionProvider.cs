using System;
using System.Linq;
using System.Web.WebPages;

using Composite.Core.Xml;

using CompositeC1Contrib.FunctionProvider;
using CompositeC1Contrib.RazorFunctions.Parser;

namespace CompositeC1Contrib.RazorFunctions.FunctionProvider
{
    public class RazorFunctionProvider : FileBasedFunctionProvider<RazorFunction>
    {
        protected override string FileExtension
        {
            get { return "cshtml"; }
        }

        protected override string Folder
        {
            get { return "Razor"; }
        }

        protected override Type BaseType
        {
            get { return typeof(CompositeC1WebPage); }
        }

        protected override Type GetReturnType(object obj)
        {
            var attr = obj.GetType().GetCustomAttributes(typeof(FunctionReturnTypeAttribute), false).Cast<FunctionReturnTypeAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.ReturnType;
            }

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
