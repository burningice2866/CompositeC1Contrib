using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class DibsProvider : PaymentProvider
    {
        private const string StatusOk = "2";
        private const string Currency = "208";
        private const string Lang = "da";
        private const string Uniqueoid = "yes";

        private string _md5Secret;
        private string _md5Secret2;
        private string _payType;

        protected override string PaymentWindowEndpoint
        {
            get { return "https://payment.architrade.com/paymentweb/start.action"; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            _md5Secret = ExtractConfigurationValue(config, "md5Secret", true);
            _md5Secret2 = ExtractConfigurationValue(config, "md5Secret2", true);
            _payType = ExtractConfigurationValue(config, "payType", false);

            base.Initialize(name, config);
        }

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            // http://tech.dibs.dk/integration_methods/flexwin/parameters/

            var merchant = MerchantId;
            var amount = (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
            var acceptUrl = ParseUrl(ContinueUrl, currentUri);
            var paytype = string.IsNullOrEmpty(_payType) ? "DK" : _payType;

            // optional parameters
            var test = IsTestMode ? "yes" : String.Empty;
            var md5Key = CalcMd5Key(merchant, order.Id, amount);
            var cancelUrl = ParseUrl(CancelUrl, currentUri);
            var callbackUrl = ParseUrl(CallbackUrl, currentUri);

            var data = new NameValueCollection
            {
                {"merchant", merchant},
                {"amount", amount},
                {"accepturl", acceptUrl},
                {"orderid", order.Id},
                {"currency", Currency},
                {"uniqueoid", Uniqueoid},
                {"test", test},
                {"md5key", md5Key},
                {"lang", Lang},
                {"cancelurl", cancelUrl},
                {"callbackurl", callbackUrl},
                {"paytype", paytype}
            };

            return GetFormPost(order, data);
        }

        public override async Task<IShopOrder> HandleCallbackAsync(HttpContextBase context)
        {
            return await Task.Run(() =>
            {
                // http://tech.dibs.dk/integration_methods/flexwin/return_pages/

                var form = context.Request.Form;

                var orderid = GetFormString("orderid", form);
                var creditCardType = GetFormString("paytype", form);
                var transact = GetFormString("transact", form);

                using (var data = new DataConnection())
                {
                    var order = data.Get<IShopOrder>().SingleOrDefault(f => f.Id == orderid);
                    if (order == null)
                    {
                        Utils.WriteLog("Error, no order with number " + orderid);

                        return null;
                    }

                    if (order.PaymentStatus == (int)PaymentStatus.Authorized)
                    {
                        Utils.WriteLog(order, "debug", "Payment is already authorized");

                        return order;
                    }

                    var statuscode = GetFormString("statuscode", form);
                    if (statuscode != StatusOk)
                    {
                        Utils.WriteLog(order, "debug",
                            "Error in status, values is " + statuscode + " but " + StatusOk + " was expected");

                        return order;
                    }

                    var amount = (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture);
                    var authkey = GetFormString("authkey", form);

                    var isValid = authkey == CalcAuthKey(transact, amount);
                    if (!isValid)
                    {
                        Utils.WriteLog(order, "debug",
                            "Error, MD5 Check doesn't match. This may just be an error in the setting or it COULD be a hacker trying to fake a completed order");

                        return order;
                    }

                    order.AuthorizationXml = OrderDataToXml(form);
                    order.CreditCardType = creditCardType;
                    order.AuthorizationTransactionId = transact;
                    order.PaymentStatus = (int)PaymentStatus.Authorized;

                    data.Update(order);

                    Utils.WriteLog(order, "authorized");

                    return order;
                }
            });
        }

        private string CalcMd5Key(string merchantId, string orderId, string amount)
        {
            var sb = new StringBuilder();

            using (var md5 = MD5.Create())
            {
                var s = _md5Secret + String.Format("merchant={0}&orderid={1}&currency={2}&amount={3}", merchantId, orderId, Currency, amount);
                var bytes = Encoding.ASCII.GetBytes(s);
                var hash = md5.ComputeHash(bytes);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                s = _md5Secret2 + sb;
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
                var s = _md5Secret + String.Format("transact={0}&amount={1}&currency={2}", transact, amount, Currency);
                var bytes = Encoding.ASCII.GetBytes(s);
                var hash = md5.ComputeHash(bytes);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                s = _md5Secret2 + sb;
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
    }
}
