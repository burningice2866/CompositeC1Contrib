using System.Web.Http;

using Owin;

namespace CompositeC1Contrib.SiteUpdate.Web
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribSiteUpdate(this IAppBuilder app, HttpConfiguration httpConfiguration)
        {
            httpConfiguration.MapHttpAttributeRoutes();
        }
    }
}
