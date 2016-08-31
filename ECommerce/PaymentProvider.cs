using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public abstract class PaymentProvider : ProviderBase
    {
        protected const string ContinueUrl = "/ecommerce/continue";
        protected const string CancelUrl = "/ecommerce/cancel";
        protected const string CallbackUrl = "/ecommerce/callback";

        private static readonly ECommerceSection Config = ECommerceSection.GetSection();

        protected abstract string PaymentWindowEndpoint { get; }

        protected string MerchantId { get; private set; }
        protected string Language { get; private set; }
        protected string PaymentMethods { get; private set; }

        protected bool IsTestMode
        {
            get { return Config.TestMode; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            MerchantId = ExtractConfigurationValue(config, "merchantId", true);
            Language = ExtractConfigurationValue(config, "language", true);
            PaymentMethods = ExtractConfigurationValue(config, "paymentMethods", false);

            config.Remove("inherits");

            base.Initialize(name, config);
        }

        public virtual Task<bool> IsPaymentAuthorizedAsync(IShopOrder order)
        {
            return Task.FromResult(order.PaymentStatus == (int)PaymentStatus.Authorized);
        }

        protected Currency ResolveCurrency(IShopOrder order)
        {
            var currency = Config.DefaultCurrency;
            if (Enum.TryParse<Currency>(order.Currency, out currency))
            {
                return currency;
            }

            return Config.DefaultCurrency;
        }

        protected decimal GetMinorCurrencyUnit(decimal amount, Currency currency)
        {
            switch (currency)
            {
                default: return amount * 100;
            }
        }

        protected static string GetFormString(string name, NameValueCollection form)
        {
            var result = String.Empty;
            if (form[name] != null)
            {
                result = form[name];
            }

            return result;
        }

        protected string GetFormPost(IShopOrder order, NameValueCollection param)
        {
            var formName = GetType().Name;

            var form = new XElement("form",
                new XAttribute("name", formName),
                new XAttribute("method", "post"),
                new XAttribute("action", PaymentWindowEndpoint));

            foreach (string name in param.Keys)
            {
                var value = param[name];

                form.Add(new XElement("input",
                    new XAttribute("name", name),
                    new XAttribute("type", "hidden"),
                    new XAttribute("value", value)
                    ));
            }

            var html = new XElement("html",
                new XElement("head",
                    new XElement("title", "Payment window")),
                new XElement("body",
                    new XAttribute("onload", String.Format("document.{0}.submit()", formName)),
                    form));

            order.WriteLog("paymentwindow generated", form.ToString());

            return html.ToString();
        }

        protected static string OrderDataToXml(NameValueCollection values)
        {
            var orderXml = new XElement("data");

            foreach (var name in values.AllKeys)
            {
                var value = values[name];
                if (!String.IsNullOrEmpty(value))
                {
                    orderXml.Add(new XElement("item",
                        new XAttribute("name", name),
                        new XAttribute("value", value)
                    ));
                }
            }

            return orderXml.ToString();
        }

        protected string ExtractConfigurationValue(NameValueCollection config, string key, bool required)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var value = config[key];
            if (value == null)
            {
                var inherits = config["inherits"];

                if (!String.IsNullOrEmpty(inherits))
                {
                    var parameters = Config.Providers[inherits].Parameters;
                    var copy = new NameValueCollection(parameters);

                    return ExtractConfigurationValue(copy, key, required);
                }
            }

            if (String.IsNullOrEmpty(value) && required)
            {
                throw new ConfigurationErrorsException(key);
            }

            config.Remove(key);

            return value;
        }

        protected static string ParseContinueUrl(IShopOrder order, Uri currentUri)
        {
            return ParseUrl(ContinueUrl + "?orderid=" + order.Id, currentUri);
        }

        protected static string ParseUrl(string url, Uri currentUri)
        {
            if (!String.IsNullOrEmpty(Config.BaseUrl))
            {
                return Config.BaseUrl + url;
            }

            return new Uri(currentUri, url).ToString();
        }

        protected static async Task<string> GetRequestContentsAsync(HttpRequestBase request)
        {
            using (var receiveStream = request.InputStream)
            {
                using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    return await readStream.ReadToEndAsync();
                }
            }
        }

        public abstract string GeneratePaymentWindow(IShopOrder order, Uri currentUri);
        public abstract Task<string> ResolveOrderIdFromRequestAsync(HttpRequestBase request);
        public abstract Task<IShopOrder> HandleCallbackAsync(HttpContextBase context);
    }
}
