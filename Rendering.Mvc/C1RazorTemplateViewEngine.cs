using System.IO;
using System.Web.Mvc;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class C1RazorTemplateViewEngine : RazorViewEngine
    {
        public C1RazorTemplateViewEngine(string location)
        {
            var viewLocations = new[] { Path.Combine(location, "{0}.cshtml") };

            PartialViewLocationFormats = viewLocations;
            ViewLocationFormats = viewLocations;
        }
    }
}
