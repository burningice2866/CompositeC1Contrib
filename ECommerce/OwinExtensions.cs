using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

using Composite.Data;
using Composite.Data.DynamicTypes;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;
using CompositeC1Contrib.ECommerce.Web;
using CompositeC1Contrib.Web;

using Owin;

namespace CompositeC1Contrib.ECommerce
{
    public static class OwinExtensions
    {
        public static void UseCompositeC1ContribECommerce(this IAppBuilder app)
        {
            RouteTable.Routes.AddGenericHandler<ECommerceHttpHandler>("ecommerce/{*pathInfo}");

            DynamicTypeManager.EnsureCreateStore(typeof(IShopOrder));
            DynamicTypeManager.EnsureCreateStore(typeof(IShopOrderLog));
            DynamicTypeManager.EnsureCreateStore(typeof(IPaymentRequest));

            Upgrade();
        }

        private static void Upgrade()
        {
            var defaultCurrency = ECommerceSection.GetSection().DefaultCurrency;

            using (var data = new DataConnection())
            {
                var update = new List<IShopOrder>();

                var orders = data.Get<IShopOrder>().Where(o => o.Currency == null || o.Currency.Length == 0).ToList();
                foreach (var order in orders)
                {
                    order.Currency = defaultCurrency.ToString();

                    update.Add(order);
                }

                data.Update<IShopOrder>(update);

                orders = data.Get<IShopOrder>().Where(o => o.AuthorizationXml != null || o.AuthorizationTransactionId != null || o.CreditCardType != null).ToList();
                foreach (var order in orders)
                {
                    var paymentRequest = data.Get<IPaymentRequest>().SingleOrDefault(p => p.ShopOrderId == order.Id);
                    if (paymentRequest == null)
                    {
                        paymentRequest = data.CreateNew<IPaymentRequest>();

                        paymentRequest.ShopOrderId = order.Id;
                    }

                    paymentRequest.ProviderName = ECommerceSection.GetSection().DefaultProvider;
                    paymentRequest.AuthorizationData = order.AuthorizationXml;
                    paymentRequest.AuthorizationTransactionId = order.AuthorizationTransactionId;
                    paymentRequest.PaymentMethod = order.CreditCardType;
                    paymentRequest.Accepted = (PaymentStatus)order.PaymentStatus == PaymentStatus.Authorized;

                    if (paymentRequest.DataSourceId.ExistsInStore)
                    {
                        data.Update(paymentRequest);
                    }
                    else
                    {
                        data.Add(paymentRequest);
                    }

                    order.AuthorizationXml = null;
                    order.AuthorizationTransactionId = null;
                    order.CreditCardType = null;

                    update.Add(order);
                }

                data.Update<IShopOrder>(update);
            }
        }
    }
}
