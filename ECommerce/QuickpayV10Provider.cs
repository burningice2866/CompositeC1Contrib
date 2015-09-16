using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private string _agreementId;
        private string _privateKey;
        private string _paymentUserApiKey;
        private string _googleTrackingId;

        private Uri _apiEndpoint;
        private string _apiUserApiKey;

        protected override string PaymentWindowEndpoint
        {
            get { return "https://payment.quickpay.net"; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            _agreementId = ExtractConfigurationValue(config, "agreementId", true);
            _privateKey = ExtractConfigurationValue(config, "privateKey", true);
            _paymentUserApiKey = ExtractConfigurationValue(config, "paymentUserApiKey", true);
            _googleTrackingId = ExtractConfigurationValue(config, "googleTrackingId", false);

            var apiEndpoint = ExtractConfigurationValue(config, "apiEndpoint", false);
            if (String.IsNullOrEmpty(apiEndpoint))
            {
                apiEndpoint = "https://api.quickpay.net";
            }

            _apiEndpoint = new Uri(apiEndpoint);

            _apiUserApiKey = ExtractConfigurationValue(config, "apiUserApiKey", true);

            base.Initialize(name, config);
        }

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            var continueUrl = ParseContinueUrl(order, currentUri);
            var cancelUrl = ParseUrl(CancelUrl, currentUri);
            var callbackUrl = ParseUrl(CallbackUrl, currentUri);

            var param = new NameValueCollection
            {
                {"version", "v10"},
                {"merchant_id", MerchantId},
                {"agreement_id", _agreementId},
                {"order_id", order.Id},
                {"amount", (order.OrderTotal*100).ToString("0", CultureInfo.InvariantCulture)},
                {"currency", "DKK"},
                {"continueurl", continueUrl},
                {"cancelurl", cancelUrl},
                {"callbackurl", callbackUrl},
                {"language", "da"}
            };

            if (!IsTestMode && !String.IsNullOrEmpty(_googleTrackingId))
            {
                param.Add("google_analytics_tracking_id", _googleTrackingId);
            }

            var checksum = Sign(param);

            param.Add("checksum", checksum);

            return GetFormPost(order, param);
        }

        public override async Task<bool> IsPaymentAuthorizedAsync(IShopOrder order)
        {
            var isAuthorized = await base.IsPaymentAuthorizedAsync(order);
            if (isAuthorized)
            {
                return true;
            }

            using (var client = new HttpClient())
            {
                var url = new Uri(_apiEndpoint, "/payments?order_id=" + order.Id);

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                var byteArray = Encoding.ASCII.GetBytes(":" + _apiUserApiKey);
                var base64 = Convert.ToBase64String(byteArray);

                request.Headers.Add("Accept-Version", "v10");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);

                var response = await client.SendAsync(request);
                var input = await response.Content.ReadAsStringAsync();

                var json = (JArray)JsonConvert.DeserializeObject(input);
                if (json.Count != 1)
                {
                    return false;
                }

                var obj = (JObject)json[0];

                return TryAuthorizeOrder(obj, out order);
            }
        }

        public override async Task<IShopOrder> HandleCallbackAsync(HttpRequestMessage request)
        {
            //http://tech.quickpay.net/api/callback/

            var input = await request.Content.ReadAsStringAsync();

            var checkSum = request.Headers.GetValues("Quickpay-Checksum-Sha256").First();
            if (checkSum != Sign(input, _privateKey))
            {
                Utils.WriteLog(null, "Error validating the checksum");

                return null;
            }

            var json = (JObject)JsonConvert.DeserializeObject(input);

            IShopOrder order;
            return TryAuthorizeOrder(json, out order) ? order : null;
        }

        private bool TryAuthorizeOrder(JObject json, out IShopOrder order)
        {
            var orderId = json["order_id"].Value<string>();

            using (var data = new DataConnection())
            {
                order = data.Get<IShopOrder>().SingleOrDefault(f => f.Id == orderId);
                if (order == null)
                {
                    Utils.WriteLog(null, "Invalid orderid " + orderId);

                    return false;
                }

                if (order.PaymentStatus == (int)PaymentStatus.Authorized)
                {
                    return true;
                }

                var accepted = json["accepted"].Value<bool>();
                if (!accepted)
                {
                    Utils.WriteLog(order, "Payment wasn't accepted");

                    return false;
                }

                var testMode = json["test_mode"].Value<bool>();
                if (testMode && !IsTestMode)
                {
                    Utils.WriteLog(order, "Payment was made with a test card but we're not in testmode");

                    return false;
                }

                var transactionId = json["id"].Value<int>();

                order.AuthorizationXml = json.ToString();
                order.AuthorizationTransactionId = transactionId.ToString();
                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                return true;
            }
        }

        private string Sign(NameValueCollection param)
        {
            var data = String.Join(" ", param.AllKeys.OrderBy(k => k).Select(k => param[k]).ToArray());

            return Sign(data, _paymentUserApiKey);
        }

        private string Sign(string data, string key)
        {
            var encoding = Encoding.UTF8;
            var hmac = new HMACSHA256(encoding.GetBytes(key));

            var bytes = hmac.ComputeHash(encoding.GetBytes(data));

            var s = new StringBuilder();

            foreach (var b in bytes)
            {
                s.Append(b.ToString("x2"));
            }

            return s.ToString();
        }
    }
}
