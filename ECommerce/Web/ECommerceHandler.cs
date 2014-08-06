using System;
using System.Linq;
using System.Web;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.Web
{
    public class ECommerceHandler : IHttpHandler
    {
        private static readonly ECommerceSection ECommerceConfig = ECommerceSection.GetSection();

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext ctx)
        {
            var output = GetOutput(ctx);
            if (!String.IsNullOrEmpty(output))
            {
                ctx.Response.Write(output);
            }
        }

        private static string GetOutput(HttpContext ctx)
        {
            var request = ctx.Request;
            var config = ECommerceSection.GetSection();
            var provider = ECommerce.DefaultProvider;

            if (request.HttpMethod == "POST")
            {
                switch (request.PathInfo)
                {
                    case "/callback": return HandleCallback(request, provider);
                    default: return null;
                }
            }

            switch (request.PathInfo)
            {
                case "/cancel": return HandleCancel(ctx, config);
                case "/continue": return HandleContinue(request);
                default: return HandleDefault(request, provider);
            }
        }

        private static string HandleCallback(HttpRequest request, PaymentProvider provider)
        {
            Utils.WriteLog(null, "Callback request recieved");

            provider.HandleCallback(request.Form);

            return null;
        }

        private static string HandleCancel(HttpContext ctx, ECommerceSection config)
        {
            Utils.WriteLog(null, "Cancel request recieved, redirecting to main page");

            var confirmNode = SiteMap.Provider.FindSiteMapNodeFromKey(config.MainPageId);
            if (confirmNode != null)
            {
                if (config.UseIFrame)
                {
                    return "<script>parent.location.href = '" + confirmNode.Url + "';</script>";
                }

                ctx.Response.Redirect(confirmNode.Url);
            }

            return null;
        }

        private static string HandleContinue(HttpRequest request)
        {
            var orderId = request.QueryString["orderid"];

            Utils.WriteLog(null, "Continue request recieved on orderid " + orderId);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId && o.PaymentStatus == (int)PaymentStatus.Authorized);
                if (order == null)
                {
                    Utils.WriteLog(null, "No authorized order with number " + orderId);

                    return null;
                }

                var orderProcessor = ECommerce.OrderProcessor;
                if (orderProcessor != null)
                {
                    orderProcessor.HandleHandlerContinue(order);
                }

                var recieptNode = SiteMap.Provider.FindSiteMapNodeFromKey(ECommerceConfig.RecieptPageId);
                var redirectUrl = recieptNode.Url + "?orderid=" + order.Id;

                request.RequestContext.HttpContext.Response.Redirect(redirectUrl);
            }

            return null;
        }

        private static string HandleDefault(HttpRequest request, PaymentProvider provider)
        {
            var qs = request.QueryString;
            var orderId = qs["orderid"];

            Utils.WriteLog(null, "Default request recieved on orderid " + orderId);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Utils.WriteLog(null, "No order with number " + orderId);

                    return null;
                }

                if (provider.IsAuthorizedRequest(qs))
                {
                    Utils.WriteLog(order, "Request is authorized, redirecting to reciept page");

                    var config = ECommerceSection.GetSection();
                    if (config.UseIFrame)
                    {
                        return "<script>parent.location.href = '/da/betaling/kvittering?orderid=" + orderId + "';</script>";
                    }
                }

                if (!String.IsNullOrEmpty(order.AuthorizationXml))
                {
                    Utils.WriteLog(order, "Order has already been handled by payment gateway");

                    return null;
                }

                Utils.WriteLog(order, "Generating payment window");

                return provider.GeneratePaymentWindow(order, request.Url);
            }
        }
    }
}
