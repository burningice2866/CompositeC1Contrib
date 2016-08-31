using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

using Composite.Data;

using CompositeC1Contrib.ECommerce;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class DummyPaymentProvider : PaymentProvider
    {
        protected override string PaymentWindowEndpoint
        {
            get { return String.Empty; }
        }

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            var links = new XElement("div",
                new XElement("form",
                    new XAttribute("target", "_blank"),
                    new XAttribute("method", "post"),
                    new XAttribute("action", CallbackUrl),

                new XElement("input",
                    new XAttribute("type", "hidden"),
                    new XAttribute("name", "orderid"),
                    new XAttribute("value", order.Id)),


                new XElement("input",
                    new XAttribute("type", "submit"),
                    new XAttribute("value", "Callback"))),

                new XElement("a",
                    new XAttribute("href", CancelUrl),
                    new XAttribute("target", "_blank"), "Cancel"),

                new XElement("a",
                    new XAttribute("href", ParseContinueUrl(order, currentUri)),
                    new XAttribute("target", "_blank"), "Continue")
                );

            var html = new XElement("html",
                new XElement("head",
                    new XElement("title", "Payment window")),
                new XElement("body", links));

            return html.ToString();
        }

        public override async Task<string> ResolveOrderIdFromRequestAsync(HttpRequestBase request)
        {
            return await Task.Run(() =>
            {
                var form = request.Form;

                return GetFormString("orderid", form);
            });
        }

        public override async Task<IShopOrder> HandleCallbackAsync(HttpContextBase context)
        {
            var orderid = await ResolveOrderIdFromRequestAsync(context.Request);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().Single(f => f.Id == orderid);
                if (order == null)
                {
                    ECommerceLog.WriteLog("Error, no order with number " + orderid);

                    return null;
                }

                var form = context.Request.Form;

                var paymentRequest = data.Get<IPaymentRequest>().Single(r => r.ShopOrderId == order.Id);

                paymentRequest.Accepted = true;
                paymentRequest.AuthorizationData = OrderDataToXml(form);
                paymentRequest.AuthorizationTransactionId = Guid.NewGuid().ToString().Substring(0, 32);
                paymentRequest.PaymentMethod = PaymentMethods;

                data.Update(paymentRequest);

                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                return order;
            }
        }
    }
}
