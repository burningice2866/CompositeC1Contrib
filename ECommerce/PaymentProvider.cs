using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

        private readonly ECommerceSection _config = ECommerceSection.GetSection();

        protected abstract string PaymentWindowEndpoint { get; }

        protected string MerchantId { get; private set; }

        protected bool IsTestMode
        {
            get { return _config.TestMode; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            MerchantId = ExtractConfigurationValue(config, "merchantId", true);

            base.Initialize(name, config);
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

            Utils.WriteLog(order, "paymentwindow generated", form.ToString());

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

        protected static string ExtractConfigurationValue(NameValueCollection config, string key, bool required)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var value = config[key];

            if (String.IsNullOrEmpty(value) && required)
            {
                throw new ConfigurationErrorsException(key);
            }

            config.Remove(key);

            return value;
        }

        protected string ParseContinueUrl(IShopOrder order, Uri currentUri)
        {
            return ParseUrl(ContinueUrl + "?orderid=" + order.Id, currentUri);
        }

        protected string ParseUrl(string url, Uri currentUri)
        {
            if (!String.IsNullOrEmpty(_config.BaseUrl))
            {
                return _config.BaseUrl + url;
            }

            return new Uri(currentUri, url).ToString();
        }

        public virtual Task<bool> IsPaymentAuthorizedAsync(IShopOrder order)
        {
            return Task.FromResult(order.PaymentStatus == (int)PaymentStatus.Authorized);
        }

        public abstract string GeneratePaymentWindow(IShopOrder order, Uri currentUri);
        public abstract Task<IShopOrder> HandleCallbackAsync(HttpRequestMessage request);
    }
}
