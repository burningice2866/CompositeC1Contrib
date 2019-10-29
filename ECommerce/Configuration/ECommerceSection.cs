using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Web.Configuration;

using CompositeC1Contrib.ECommerce.PaymentProviders;

namespace CompositeC1Contrib.ECommerce.Configuration
{
    public class ECommerceSection : ConfigurationSection
    {
        private const string ConfigPath = "compositeC1Contrib/eCommerce";

        private static readonly ConcurrentDictionary<string, PaymentProviderBase> ProvidersInstances = new ConcurrentDictionary<string, PaymentProviderBase>();

        [ConfigurationProperty("defaultProvider")]
        public string DefaultProvider
        {
            get => (string)this["defaultProvider"];
            set => this["defaultProvider"] = value;
        }

        [ConfigurationProperty("baseUrl", IsRequired = false)]
        public string BaseUrl
        {
            get => (string)this["baseUrl"];
            set => this["baseUrl"] = value;
        }

        [ConfigurationProperty("mainPageId", IsRequired = false)]
        public string MainPageId
        {
            get => (string)this["mainPageId"];
            set => this["mainPageId"] = value;
        }

        [ConfigurationProperty("receiptPageId", IsRequired = false)]
        public string ReceiptPageId
        {
            get => (string)this["receiptPageId"];
            set => this["receiptPageId"] = value;
        }

        [ConfigurationProperty("testMode", DefaultValue = false)]
        public bool TestMode
        {
            get => (bool)this["testMode"];
            set => this["testMode"] = value;
        }

        [ConfigurationProperty("useIFrame", DefaultValue = false)]
        public bool UseIFrame
        {
            get => (bool)this["useIFrame"];
            set => this["useIFrame"] = value;
        }

        [ConfigurationProperty("orderProcessor", IsRequired = false)]
        public string OrderProcessor
        {
            get => (string)this["orderProcessor"];
            set => this["orderProcessor"] = value;
        }

        [ConfigurationProperty("defaultCurrency", IsRequired = false, DefaultValue = Currency.DKK)]
        public Currency DefaultCurrency
        {
            get => (Currency)this["defaultCurrency"];
            set => this["defaultCurrency"] = value;
        }

        [ConfigurationProperty("minimumOrderIdLength", IsRequired = false, DefaultValue = -1)]
        public int MinimumOrderIdLength
        {
            get => (int)this["minimumOrderIdLength"];
            set => this["minimumOrderIdLength"] = value;
        }

        [ConfigurationProperty("orderIdPrefix", IsRequired = false)]
        public string OrderIdPrefix
        {
            get => (string)this["orderIdPrefix"];
            set => this["orderIdPrefix"] = value;
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers => (ProviderSettingsCollection)base["providers"];

        public PaymentProviderBase GetProviderInstance(string name)
        {
            return ProvidersInstances.GetOrAdd(name, s =>
            {
                var settings = Providers[name];

                var type = Type.GetType(settings.Type);
                if (type == null)
                {
                    return null;
                }

                try
                {
                    return (PaymentProviderBase)ProvidersHelper.InstantiateProvider(settings, type);
                }
                catch (Exception e)
                {
                    ECommerceLog.WriteLog("Error instantiating provider", e);

                    return null;
                }
            });
        }

        public static ECommerceSection GetSection()
        {
            return (ECommerceSection)ConfigurationManager.GetSection(ConfigPath);
        }
    }
}
