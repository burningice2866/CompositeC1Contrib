using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CompositeC1Contrib.ECommerce
{
    public class QuickpayV10Provider : PaymentProvider
    {
        public string AgreementId;
        public string ApiKey;
        public string GoogleTrackingId;

        public override void Initialize(string name, NameValueCollection config)
        {
            AgreementId = ExtractConfigurationValue(config, "agreementId", true);
            ApiKey = ExtractConfigurationValue(config, "apiKey", true);
            GoogleTrackingId = ExtractConfigurationValue(config, "googleTrackingId", false);

            base.Initialize(name, config);
        }

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            var continueurl = ParseUrl(ContinueUrl + "?orderid=" + order.Id, currentUri);
            var cancelurl = ParseUrl(CancelUrl, currentUri);
            var callbackurl = ParseUrl(CallbackUrl, currentUri);

            var param = new NameValueCollection();
            param.Add("version", "v10");
            param.Add("merchant_id", MerchantId);
            param.Add("agreement_id", AgreementId);
            param.Add("order_id", order.Id);
            param.Add("amount", (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture));
            param.Add("currency", "DKK");
            param.Add("continueurl", continueurl);
            param.Add("cancelurl", cancelurl);
            param.Add("callbackurl", callbackurl);
            param.Add("language", "da");

            if (!IsTestMode && !String.IsNullOrEmpty(GoogleTrackingId))
            {
                param.Add("google_analytics_tracking_id", GoogleTrackingId);
            }

            var checksum = Sign(param, ApiKey);

            param.Add("checksum", checksum);

            return GetFormPost("quickpayv10", "https://payment.quickpay.net", order, param);
        }

        public override async Task<IShopOrder> HandleCallbackAsync(HttpRequestMessage request)
        {
            //http://tech.quickpay.net/api/callback/

            var input = await request.Content.ReadAsStringAsync();
            var json = (JObject)JsonConvert.DeserializeObject(input);

            var orderId = json["order_id"].Value<string>();

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(f => f.Id == orderId);
                if (order == null)
                {
                    Utils.WriteLog(null, "Error, no order with number " + orderId);

                    return order;
                }

                var accepted = json["accepted"].Value<bool>();
                if (!accepted)
                {
                    Utils.WriteLog(order, "Request wasn't accepted");

                    return order;
                }

                var transactionId = json["id"].Value<int>();

                order.AuthorizationXml = json.ToString();
                order.AuthorizationTransactionId = transactionId.ToString();
                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                return order;
            }
        }

        private string Sign(NameValueCollection param, string apiKey)
        {
            var encoding = Encoding.UTF8;
            var hmac = new HMACSHA256(encoding.GetBytes(apiKey));

            var result = String.Join(" ", param.AllKeys.OrderBy(k => k).Select(k => param[k]).ToArray());
            var bytes = hmac.ComputeHash(encoding.GetBytes(result));

            var s = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                s.Append(bytes[i].ToString("x2"));
            }

            return s.ToString();
        }
    }
}
