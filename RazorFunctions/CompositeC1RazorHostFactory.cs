using System.Web.WebPages.Razor;

namespace CompositeC1Contrib.RazorFunctions
{
    public class CompositeC1RazorHostFactory : WebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            return new CompositeC1RazorHost(virtualPath, physicalPath);
        }
    }
}
