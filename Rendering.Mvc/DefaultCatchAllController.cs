using System.Web.Mvc;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class DefaultCatchAllController : C1Controller
    {
        public ActionResult Get()
        {
            return C1View();
        }
    }
}
