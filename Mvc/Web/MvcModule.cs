using System.Web;
using System.Web.Routing;

using CompositeC1Contrib.Web.Mvc;

namespace CompositeC1Contrib.Web
{
    public class MvcModule : IHttpModule
    {
        public void Dispose()
        {

        }

        public void Init(HttpApplication ctx)
        {
            var routes = RouteTable.Routes;
            routes.Add(new ContentRoute());
        }
    }
}
