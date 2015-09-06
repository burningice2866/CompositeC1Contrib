using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
using System.Net.Http;
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
        private ECommerceSection config = ECommerceSection.GetSection();

        protected string ContinueUrl = "/ecommerce/continue";
        protected string CancelUrl = "/ecommerce/cancel";
        protected string CallbackUrl = "/ecommerce/callback";

        public string MerchantId { get; protected set; }

        public bool IsTestMode
        {
            get { return config.TestMode; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            MerchantId = ExtractConfigurationValue(config, "merchantId", true);

            base.Initialize(name, config);
        }

        protected string GetFormString(string name, NameValueCollection form)
        {
            string result = String.Empty;
            if (form[name] != null)
            {
                result = form[name];
            }

            return result;
        }

        protected string GetFormPost(string formName, string action, IShopOrder order, NameValueCollection param)
        {
            var sb = new StringBuilder();

            sb.Append("<html><head></head>");
            sb.AppendFormat("<body onload=\"document.{0}.submit()\">", formName);
            sb.AppendFormat("<form name=\"{0}\" method=\"post\" action=\"{1}\">", formName, action);

            foreach (string name in param.Keys)
            {
                sb.AppendFormat("<input name=\"{0}\" type=\"hidden\" value=\"{1}\" />", HttpUtility.HtmlEncode(name), HttpUtility.HtmlEncode(param[name]));
            }

            sb.Append("</form></body></html>");

            Utils.WriteLog(order, "Payment window generated with the following data " + OrderDataToXml(param));

            return sb.ToString();
        }

        protected string OrderDataToXml(NameValueCollection values)
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

        protected NameValueCollection OrderXmlToData(string xml)
        {
            var orderData = new NameValueCollection();
            var orderXml = XElement.Parse(xml);
            var dataElements = orderXml.Elements().Where(item => item.Attribute("name") != null && item.Attribute("value") != null);

            foreach (var item in dataElements)
            {
                orderData.Add((item.Attribute("name")).Value, item.Attribute("value").Value);
            }

            return orderData;
        }

        protected string ExtractConfigurationValue(NameValueCollection config, string key, bool required)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var value = config[key];

            if (required)
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ConfigurationErrorsException(key);
                }
            }

            config.Remove(key);

            return value;
        }

        public virtual bool IsAuthorizedRequest(IDictionary<string, string> qs)
        {
            return false;
        }

        protected string ParseUrl(string url, Uri currentUri)
        {
            if (!String.IsNullOrEmpty(config.BaseUrl))
            {
                return config.BaseUrl + url;
            }

            return new Uri(currentUri, url).ToString();
        }

        public abstract string GeneratePaymentWindow(IShopOrder order, Uri currentUri);
        public abstract Task<IShopOrder> HandleCallbackAsync(HttpRequestMessage request);
    }
}
