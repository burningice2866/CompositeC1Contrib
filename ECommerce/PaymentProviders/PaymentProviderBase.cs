using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.PaymentProviders
{
    public abstract class PaymentProviderBase : ProviderBase
    {
        private static readonly ECommerceSection Config = ECommerceSection.GetSection();
        private static readonly IOrderProcessor OrderProcessor = ECommerce.OrderProcessor;

        protected const string ContinueUrl = "/ecommerce/continue";
        protected const string CancelUrl = "/ecommerce/cancel";
        protected const string CallbackUrl = "/ecommerce/callback";

        protected abstract string PaymentWindowEndpoint { get; }

        protected string MerchantId { get; private set; }
        protected string Language { get; private set; }
        protected string PaymentMethods { get; private set; }

        protected bool IsTestMode => Config.TestMode;

        public override void Initialize(string name, NameValueCollection config)
        {
            Language = ExtractConfigurationValue(config, "language", false);
            if (String.IsNullOrEmpty(Language))
            {
                Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }

            MerchantId = ExtractConfigurationValue(config, "merchantId", true);
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
            return Enum.TryParse<Currency>(order.Currency, out var currency) ? currency : Config.DefaultCurrency;
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
                    new XAttribute("onload", $"document.{formName}.submit()"),
                    form));

            order.WriteLog("Payment window generated", form.ToString());

            return html.ToString();
        }

        protected string ExtractConfigurationValue(NameValueCollection config, string key, bool required)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
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

        public abstract string GeneratePaymentWindow(IShopOrder order, IPaymentRequest paymentRequest, Uri currentUri);

        public async Task<IShopOrder> HandleCallbackAsync(HttpContextBase context)
        {
            var order = await ResolveOrderAsync(context);
            if (order == null)
            {
                return null;
            }

            if (order.PaymentStatus == (int)PaymentStatus.Authorized)
            {
                order.WriteLog("debug", "Payment is already authorized");

                return order;
            }

            var handled = OrderProcessor.HandleCallback(context, order);
            if (!handled)
            {
                HandleCallbackInternal(context, order);
            }

            return order;
        }

        protected virtual async Task<IShopOrder> ResolveOrderAsync(HttpContextBase context)
        {
            var orderId = await ResolveOrderIdFromRequestAsync(context.Request);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(f => f.Id == orderId);
                if (order == null)
                {
                    ECommerceLog.WriteLog("Error, no order with id " + orderId);
                }

                return order;
            }
        }

        public abstract Task<string> ResolveOrderIdFromRequestAsync(HttpRequestBase request);

        protected virtual void HandleCallbackInternal(HttpContextBase context, IShopOrder order) { }
    }
}
