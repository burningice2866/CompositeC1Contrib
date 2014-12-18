using System.Web.Mvc;

using CompositeC1Contrib.Rendering.Mvc.Routing;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class DefaultCatchAllController : C1Controller
    {
        [C1PageTemplateRoute("{*url}")]
        public ActionResult Get()
        {
            return C1View();
        }
    }
}
