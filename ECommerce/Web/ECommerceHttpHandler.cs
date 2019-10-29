using System.Threading.Tasks;
using System.Web;

namespace CompositeC1Contrib.ECommerce.Web
{
    public class ECommerceHttpHandler : HttpTaskAsyncHandler
    {
        public override bool IsReusable => true;

        public override Task ProcessRequestAsync(HttpContext context)
        {
            var contextBase = new HttpContextWrapper(context);
            var pathInfo = (string)contextBase.Request.RequestContext.RouteData.Values["pathInfo"];

            var executor = new ECommerceRequestExecutor(contextBase);

            if (context.Request.HttpMethod == "POST")
            {
                switch (pathInfo)
                {
                    case "callback": return executor.HandleCallback();
                }
            }

            switch (pathInfo)
            {
                case "cancel": return executor.HandleCancel();
                case "continue": return executor.HandleContinue();
                default: return executor.HandleDefault();
            }
        }
    }
}
