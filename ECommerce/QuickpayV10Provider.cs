using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CompositeC1Contrib.ECommerce
{
    public class QuickpayV10Provider : PaymentProvider
    {
        protected string AgreementId { get; private set; }
        protected string PrivateKey { get; private set; }
        protected string PaymentUserApiKey { get; private set; }
        protected string GoogleTrackingId { get; private set; }

        protected Uri ApiEndpoint { get; private set; }
        protected string ApiUserApiKey { get; private set; }

        protected override string PaymentWindowEndpoint
        {
            get { return "https://payment.quickpay.net"; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            AgreementId = ExtractConfigurationValue(config, "agreementId", true);
            PrivateKey = ExtractConfigurationValue(config, "privateKey", true);
            PaymentUserApiKey = ExtractConfigurationValue(config, "paymentUserApiKey", true);
            GoogleTrackingId = ExtractConfigurationValue(config, "googleTrackingId", false);

            var apiEndpoint = ExtractConfigurationValue(config, "apiEndpoint", false);
            if (String.IsNullOrEmpty(apiEndpoint))
            {
                apiEndpoint = "https://api.quickpay.net";
            }

            ApiEndpoint = new Uri(apiEndpoint);

            ApiUserApiKey = ExtractConfigurationValue(config, "apiUserApiKey", true);
            
            base.Initialize(name, config);
        }

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            var currency = ResolveCurrency(order);
            var amount = GetMinorCurrencyUnit(order.OrderTotal, currency).ToString("0", CultureInfo.InvariantCulture);

            var continueUrl = ParseContinueUrl(order, currentUri);
            var cancelUrl = ParseUrl(CancelUrl, currentUri);
            var callbackUrl = ParseUrl(CallbackUrl, currentUri);

            var param = new NameValueCollection
            {
                {"version", "v10"},
                {"merchant_id", MerchantId},
                {"agreement_id", AgreementId},
                {"order_id", order.Id},
                {"amount", amount},
                {"currency", currency.ToString()},
                {"continueurl", continueUrl},
                {"cancelurl", cancelUrl},
                {"callbackurl", callbackUrl},
                {"language", Language}
            };

            if (!String.IsNullOrEmpty(PaymentMethods))
            {
                param.Add("payment_methods", PaymentMethods);
            }

            if (!IsTestMode && !String.IsNullOrEmpty(GoogleTrackingId))
            {
                param.Add("google_analytics_tracking_id", GoogleTrackingId);
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
                var url = new Uri(ApiEndpoint, "/payments?order_id=" + order.Id);

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                var byteArray = Encoding.ASCII.GetBytes(":" + ApiUserApiKey);
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

        public override async Task<string> ResolveOrderIdFromRequestAsync(HttpRequestBase request)
        {
            var input = await GetRequestContentsAsync(request);

            var json = (JObject)JsonConvert.DeserializeObject(input);
            var orderId = json["order_id"].Value<string>();

            return orderId;
        }

        public override async Task<IShopOrder> HandleCallbackAsync(HttpContextBase context)
        {
            //http://tech.quickpay.net/api/callback/

            var input = await GetRequestContentsAsync(context.Request);

            var checkSum = context.Request.Headers.Get("Quickpay-Checksum-Sha256");
            if (checkSum != Sign(input, PrivateKey))
            {
                ECommerceLog.WriteLog("Error validating the checksum");

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
                    ECommerceLog.WriteLog("Invalid orderid " + orderId);

                    return false;
                }

                if (order.PaymentStatus == (int)PaymentStatus.Authorized)
                {
                    order.WriteLog("debug", "Payment is already authorized");

                    return true;
                }

                var accepted = json["accepted"].Value<bool>();
                if (!accepted)
                {
                    order.WriteLog("debug", "Payment wasn't accepted");

                    return false;
                }

                var testMode = json["test_mode"].Value<bool>();
                if (testMode && !IsTestMode)
                {
                    order.WriteLog("debug", "Payment was made with a test card but we're not in testmode");

                    return false;
                }

                var paymentRequest = data.Get<IPaymentRequest>().Single(r => r.ShopOrderId == orderId);

                paymentRequest.Accepted = true;
                paymentRequest.AuthorizationData = json.ToString();
                paymentRequest.AuthorizationTransactionId = json["id"].Value<int>().ToString();
                paymentRequest.PaymentMethod = json["metadata"]["type"].Value<string>();

                data.Update(paymentRequest);

                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                order.WriteLog("authorized");

                return true;
            }
        }

        private string Sign(NameValueCollection param)
        {
            var data = String.Join(" ", param.AllKeys.OrderBy(k => k).Select(k => param[k]).ToArray());

            return Sign(data, PaymentUserApiKey);
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
