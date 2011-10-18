using System.Web.WebPages.Razor;

namespace CompositeC1Contrib.RazorFunctions
{
    public class RazorHostFactory : WebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            return new RazorHost(virtualPath, physicalPath);
        }
    }
}
