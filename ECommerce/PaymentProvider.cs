using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public abstract class PaymentProvider : ProviderBase
    {
        protected string ContinueUrl = "/ecommerce/continue";
        protected string CancelUrl = "/ecommerce/cancel";
        protected string CallbackUrl = "/ecommerce/callback";

        public string MerchantId { get; private set; }
        public string MD5Secret { get; private set; }

        public bool IsTestMode
        {
            get
            {
                var config = ECommerceSection.GetSection();

                return config.TestMode;
            }
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

        protected string GetFormPost(string formName, string action, NameValueCollection values)
        {
            var sb = new StringBuilder();

            sb.Append("<html><head></head>");
            sb.AppendFormat("<body onload=\"document.{0}.submit()\">", formName);
            sb.AppendFormat("<form name=\"{0}\" method=\"post\" action=\"{1}\">", formName, action);

            foreach (string name in values.Keys)
            {
                sb.AppendFormat("<input name=\"{0}\" type=\"hidden\" value=\"{1}\" />", HttpUtility.HtmlEncode(name), HttpUtility.HtmlEncode(values[name]));
            }

            sb.Append("</form></body></html>");

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

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            MerchantId = config["merchantId"];
            if (String.IsNullOrEmpty(MerchantId))
            {
                throw new ConfigurationErrorsException("merchantId");
            }

            config.Remove("merchantId");

            MD5Secret = config["md5Secret"];
            if (String.IsNullOrEmpty(MD5Secret))
            {
                throw new ConfigurationErrorsException("md5Secret");
            }

            config.Remove("md5Secret");

            base.Initialize(name, config);
        }

        public virtual bool IsAuthorizedRequest(NameValueCollection qs)
        {
            return false;
        }

        public abstract string GeneratePaymentWindow(IShopOrder order, Uri currentUri);
        public abstract void HandleCallback(NameValueCollection form);
    }
}
