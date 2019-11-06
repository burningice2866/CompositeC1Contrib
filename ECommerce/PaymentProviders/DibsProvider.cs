using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.PaymentProviders
{
    public class DibsProvider : PaymentProviderBase
    {
        private const string StatusOk = "2";
        private const string UniqueOID = "yes";

        protected string Md5Secret { get; private set; }
        protected string Md5Secret2 { get; private set; }

        protected override string PaymentWindowEndpoint => "https://payment.architrade.com/paymentweb/start.action";

        public override void Initialize(string name, NameValueCollection config)
        {
            Md5Secret = ExtractConfigurationValue(config, "md5Secret", false);
            Md5Secret2 = ExtractConfigurationValue(config, "md5Secret2", false);

            base.Initialize(name, config);
        }

        public override string GeneratePaymentWindow(IShopOrder order, IPaymentRequest paymentRequest, Uri currentUri)
        {
            // http://tech.dibs.dk/integration_methods/flexwin/parameters/

            var currency = ResolveCurrency(order);
            var amount = GetMinorCurrencyUnit(order.OrderTotal, currency).ToString("0", CultureInfo.InvariantCulture);
            var acceptUrl = ParseUrl(ContinueUrl, currentUri);
            var paytype = String.IsNullOrEmpty(PaymentMethods) ? "DK" : PaymentMethods;

            // optional parameters
            var test = IsTestMode ? "yes" : String.Empty;
            var cancelUrl = ParseUrl(paymentRequest.CancelUrl ?? CancelUrl, currentUri);
            var callbackUrl = ParseUrl(CallbackUrl, currentUri);

            var data = new NameValueCollection
            {
                {"merchant", MerchantId},
                {"amount", amount},
                {"accepturl", acceptUrl},
                {"orderid", order.Id},
                {"currency", ((int)currency).ToString()},
                {"uniqueoid", UniqueOID},
                {"test", test},
                {"lang", Language},
                {"cancelurl", cancelUrl},
                {"callbackurl", callbackUrl},
                {"paytype", paytype}
            };

            if (TryCalcMd5Key(MerchantId, order.Id, currency, amount, out var md5Key))
            {
                data.Add("md5key", md5Key);
            }

            return GetFormPost(order, data);
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
            // http://tech.dibs.dk/integration_methods/flexwin/return_pages/

            var form = context.Request.Form;

            var statuscode = GetFormString("statuscode", form);
            if (statuscode != StatusOk)
            {
                order.WriteLog("debug", "Error in status, values is " + statuscode + " but " + StatusOk + " was expected");

                return;
            }

            var authkey = GetFormString("authkey", form);
            var transact = GetFormString("transact", form);
            var currency = ResolveCurrency(order);
            var amount = GetMinorCurrencyUnit(order.OrderTotal, currency).ToString("0", CultureInfo.InvariantCulture);

            if (HasMd5Key())
            {
                var isValid = authkey == CalcAuthKey(transact, currency, amount);
                if (!isValid)
                {
                    order.WriteLog("debug", "Error, MD5 Check doesn't match. This may just be an error in the setting or it COULD be a hacker trying to fake a completed order");

                    return;
                }
            }

            using (var data = new DataConnection())
            {
                var paymentRequest = order.GetPaymentRequest();

                paymentRequest.Accepted = true;
                paymentRequest.AuthorizationData = form.ToXml();
                paymentRequest.AuthorizationTransactionId = transact;
                paymentRequest.PaymentMethod = GetFormString("paytype", form);

                data.Update(paymentRequest);

                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                order.WriteLog("authorized");
            }
        }

        private bool HasMd5Key()
        {
            return !String.IsNullOrEmpty(Md5Secret) && !String.IsNullOrEmpty(Md5Secret2);
        }

        private bool TryCalcMd5Key(string merchantId, string orderId, Currency currency, string amount, out string md5Key)
        {
            if (!HasMd5Key())
            {
                md5Key = null;

                return false;
            }

            var sb = new StringBuilder();

            using (var md5 = MD5.Create())
            {
                var s = Md5Secret + $"merchant={merchantId}&orderid={orderId}&currency={(int)currency}&amount={amount}";
                var bytes = Encoding.ASCII.GetBytes(s);
                var hash = md5.ComputeHash(bytes);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                s = Md5Secret2 + sb;
                bytes = Encoding.ASCII.GetBytes(s);
                hash = md5.ComputeHash(bytes);

                sb.Length = 0;

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
            }

            md5Key = sb.ToString();

            return true;
        }

        private string CalcAuthKey(string transact, Currency currency, string amount)
        {
            var sb = new StringBuilder();

            using (var md5 = MD5.Create())
            {
                var s = Md5Secret + $"transact={transact}&amount={amount}&currency={(int)currency}";
                var bytes = Encoding.ASCII.GetBytes(s);
                var hash = md5.ComputeHash(bytes);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                s = Md5Secret2 + sb;
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
