using System.Configuration;

namespace CompositeC1Contrib.ECommerce.Configuration
{
    public class ECommerceSection : ConfigurationSection
    {
        private const string ConfigPath = "compositeC1Contrib/eCommerce";

        [ConfigurationProperty("defaultProvider")]
        public string DefaultProvider
        {
            get { return (string)this["defaultProvider"]; }
            set { this["defaultProvider"] = value; }
        }

        [ConfigurationProperty("baseUrl", IsRequired = false)]
        public string BaseUrl
        {
            get { return (string)this["baseUrl"]; }
            set { this["baseUrl"] = value; }
        }

        [ConfigurationProperty("mainPageId", IsRequired = false)]
        public string MainPageId
        {
            get { return (string)this["mainPageId"]; }
            set { this["mainPageId"] = value; }
        }

        [ConfigurationProperty("receiptPageId", IsRequired = false)]
        public string ReceiptPageId
        {
            get { return (string)this["receiptPageId"]; }
            set { this["receiptPageId"] = value; }
        }

        [ConfigurationProperty("testMode", DefaultValue = false)]
        public bool TestMode
        {
            get { return (bool)this["testMode"]; }
            set { this["testMode"] = value; }
        }

        [ConfigurationProperty("useIFrame", DefaultValue = false)]
        public bool UseIFrame
        {
            get { return (bool)this["useIFrame"]; }
            set { this["useIFrame"] = value; }
        }

        [ConfigurationProperty("orderProcessor", IsRequired = false)]
        public string OrderProcessor
        {
            get { return (string)this["orderProcessor"]; }
            set { this["orderProcessor"] = value; }
        }

        [ConfigurationProperty("minimumOrderNumberLength", IsRequired = false, DefaultValue = -1)]
        public int MinimumOrderNumberLength
        {
            get { return (int)this["minimumOrderNumberLength"]; }
            set { this["minimumOrderNumberLength"] = value; }
        }

        [ConfigurationProperty("orderNumberPrefix", IsRequired = false)]
        public string OrderNumberPrefix
        {
            get { return (string)this["orderNumberPrefix"]; }
            set { this["orderNumberPrefix"] = value; }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get { return (ProviderSettingsCollection)base["providers"]; }
        }

        public static ECommerceSection GetSection()
        {
            return (ECommerceSection)ConfigurationManager.GetSection(ConfigPath);
        }
    }
}
