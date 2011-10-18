using System;
using System.IO;
using System.Web.Razor;
using System.Web.WebPages.Razor;

namespace CompositeC1Contrib.RazorFunctions
{
    public class RazorHost : WebPageRazorHost
    {
        public RazorHost(string virtualPath)
            : base(virtualPath)
        {
        }

        public RazorHost(string virtualPath, string physicalPath)
            : base(virtualPath, physicalPath)
        {
        }

        protected override RazorCodeLanguage GetCodeLanguage()
        {
            if (String.Equals(Path.GetExtension(base.VirtualPath), ".razor", StringComparison.OrdinalIgnoreCase))
            {
                return new CSharpRazorCodeLanguage();
            }

            return base.GetCodeLanguage();
        }
    }
}
