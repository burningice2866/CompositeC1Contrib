using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
        public string MD5Secret2 { get; private set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            GatewayUrl = config["gatewayUrl"];
            config.Remove("gatewayUrl");

            MD5Secret2 = config["md5Secret2"];
            if (String.IsNullOrEmpty(MD5Secret2))
            {
                throw new ConfigurationErrorsException("md5Secret2");
            }

            config.Remove("md5Secret2");

            base.Initialize(name, config);
        }

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            var schemeAndServer = currentUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);

            //http://tech.dibs.dk/integration_methods/flexwin/parameters/

            var merchant = MerchantId;
            var amount = (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
            var accepturl = schemeAndServer + ContinueUrl;

            // optional parameters
            var test = IsTestMode ? "yes" : String.Empty;
            var md5key = CalcMD5Key(merchant, order.Id, amount);
            var cancelurl = schemeAndServer + CancelUrl;
            var callbackurl = schemeAndServer + CallbackUrl;

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

            Utils.WriteLog(order, "FlexWin window generated with the following data " + OrderDataToXml(data));

            return GetFormPost("FlexWin", "https://payment.architrade.com/paymentweb/start.action", data);
        }

        public override void HandleCallback(NameValueCollection form)
        {
            //http://tech.dibs.dk/integration_methods/flexwin/return_pages/

            var orderid = GetFormString("orderid", form);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().Single(f => f.Id == orderid);
                if (order == null)
                {
                    Utils.WriteLog(null, "Error, no order with number " + orderid);

                    return;
                }

                order.AuthorizationXml = OrderDataToXml(form);

                data.Update(order);

                var statuscode = GetFormString("statuscode", form);
                if (statuscode != StatusOk)
                {
                    Utils.WriteLog(order, "Error in status, values is " + statuscode + " but " + StatusOk + " was expected");

                    return;
                }

                var amount = (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
                var authkey = GetFormString("authkey", form);
                var transact = GetFormString("transact", form);

                var isValid = authkey == CalcAuthKey(transact, amount);
                if (!isValid)
                {
                    Utils.WriteLog(order, "Error, MD5 Check doesn't match. This may just be an error in the setting or it COULD be a hacker trying to fake a completed order");

                    return;
                }

                order.CreditCardType = GetFormString("paytype", form); ;
                order.AuthorizationTransactionId = transact;
                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                Utils.WriteLog(order, "Authorized with the following transactionid " + order.AuthorizationTransactionId);
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

        public override bool IsAuthorizedRequest(NameValueCollection qs)
        {
            var authkey = qs["authkey"];

            return !String.IsNullOrEmpty(authkey);
        }
    }
}
