using System;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.PaymentProviders
{
    public class DummyPaymentProvider : PaymentProviderBase
    {
        protected override string PaymentWindowEndpoint => String.Empty;

        public override string GeneratePaymentWindow(IShopOrder order, IPaymentRequest paymentRequest, Uri currentUri)
        {
            var cancelUrl = ParseUrl(paymentRequest.CancelUrl ?? CancelUrl, currentUri);

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
                    new XAttribute("href", cancelUrl),
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

        protected override void HandleCallbackInternal(HttpContextBase context, IShopOrder order)
        {
            using (var data = new DataConnection())
            {
                var form = context.Request.Form;

                var paymentRequest = order.GetPaymentRequest();

                paymentRequest.Accepted = true;
                paymentRequest.AuthorizationData = form.ToXml();
                paymentRequest.AuthorizationTransactionId = Guid.NewGuid().ToString().Substring(0, 32);
                paymentRequest.PaymentMethod = PaymentMethods;

                data.Update(paymentRequest);

                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);
            }
        }
    }
}
