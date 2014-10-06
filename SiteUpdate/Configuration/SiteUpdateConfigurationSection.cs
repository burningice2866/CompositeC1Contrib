using System.Configuration;

namespace CompositeC1Contrib.SiteUpdate.Configuration
{
    public class SiteUpdateConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("storeLocation", IsRequired = true)]
        public string StoreLocation
        {
            get { return (string)this["storeLocation"]; }
            set { this["storeLocation"] = value; }
        }

        public static SiteUpdateConfigurationSection GetSection()
        {
            return ConfigurationManager.GetSection("compositeC1Contrib/siteUpdate") as SiteUpdateConfigurationSection;
        }
    }
}
