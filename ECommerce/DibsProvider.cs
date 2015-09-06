using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class DibsProvider : PaymentProvider
    {
        private const string StatusOk = "2";
        private const string Currency = "208";
        private const string Paytype = "DK";
        private const string Lang = "da";
        private const string Uniqueoid = "yes";

        public string GatewayUrl { get; private set; }
        public string MD5Secret { get; protected set; }
        public string MD5Secret2 { get; private set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            MD5Secret = ExtractConfigurationValue(config, "md5Secret", true);
            MD5Secret2 = ExtractConfigurationValue(config, "md5Secret2", true);
            GatewayUrl = ExtractConfigurationValue(config, "gatewayUrl", false);

            base.Initialize(name, config);
        }

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            // http://tech.dibs.dk/integration_methods/flexwin/parameters/

            var merchant = MerchantId;
            var amount = (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
            var accepturl = ParseUrl(ContinueUrl, currentUri);

            // optional parameters
            var test = IsTestMode ? "yes" : String.Empty;
            var md5key = CalcMD5Key(merchant, order.Id, amount);
            var cancelurl = ParseUrl(CancelUrl, currentUri);
            var callbackurl = ParseUrl(CallbackUrl, currentUri);

            var data = new NameValueCollection
            {
                {"merchant", merchant},
                {"amount", amount},
                {"accepturl", accepturl},
                {"orderid", order.Id},
                {"currency", Currency},
                {"uniqueoid", Uniqueoid},
                {"test", test},
                {"md5key", md5key},
                {"lang", Lang},
                {"cancelurl", cancelurl},
                {"callbackurl", callbackurl},
                {"paytype", Paytype}
            };

            return GetFormPost("FlexWin", "https://payment.architrade.com/paymentweb/start.action", order, data);
        }

        public override async Task<IShopOrder> HandleCallbackAsync(HttpRequestMessage request)
        {
            // http://tech.dibs.dk/integration_methods/flexwin/return_pages/

            var form = await request.Content.ReadAsFormDataAsync();

            var orderid = GetFormString("orderid", form);
            var creditCardType = GetFormString("paytype", form);
            var transact = GetFormString("transact", form);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(f => f.Id == orderid);
                if (order == null)
                {
                    Utils.WriteLog(null, "Error, no order with number " + orderid);

                    return order;
                }

                var statuscode = GetFormString("statuscode", form);
                if (statuscode != StatusOk)
                {
                    Utils.WriteLog(order, "Error in status, values is " + statuscode + " but " + StatusOk + " was expected");

                    return order;
                }

                var amount = (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
                var authkey = GetFormString("authkey", form);

                var isValid = authkey == CalcAuthKey(transact, amount);
                if (!isValid)
                {
                    Utils.WriteLog(order, "Error, MD5 Check doesn't match. This may just be an error in the setting or it COULD be a hacker trying to fake a completed order");

                    return order;
                }

                order.AuthorizationXml = OrderDataToXml(form);
                order.CreditCardType = creditCardType;
                order.AuthorizationTransactionId = transact;
                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                return order;
            }
        }

        private string CalcMD5Key(string merchantId, string orderId, string amount)
        {
            var sb = new StringBuilder();

            using (var md5 = MD5.Create())
            {
                var s = MD5Secret + String.Format("merchant={0}&orderid={1}&currency={2}&amount={3}", merchantId, orderId, Currency, amount);
                var bytes = Encoding.ASCII.GetBytes(s);
                var hash = md5.ComputeHash(bytes);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                s = MD5Secret2 + sb;
                bytes = Encoding.ASCII.GetBytes(s);
                hash = md5.ComputeHash(bytes);

                sb.Length = 0;

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
            }

            return sb.ToString();
        }

        private string CalcAuthKey(string transact, string amount)
        {
            var sb = new StringBuilder();

            using (var md5 = MD5.Create())
            {
                var s = MD5Secret + String.Format("transact={0}&amount={1}&currency={2}", transact, amount, Currency);
                var bytes = Encoding.ASCII.GetBytes(s);
                var hash = md5.ComputeHash(bytes);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                s = MD5Secret2 + sb;
                bytes = Encoding.ASCII.GetBytes(s);
                hash = md5.ComputeHash(bytes);

                sb.Length = 0;

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
            }

            return sb.ToString();
        }

        public override bool IsAuthorizedRequest(IDictionary<string, string> qs)
        {
            var authkey = qs["authkey"];

            return !String.IsNullOrEmpty(authkey);
        }
    }
}
