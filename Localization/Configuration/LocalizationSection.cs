using System.Configuration;

namespace CompositeC1Contrib.Localization.Configuration
{
    public class LocalizationSection : ConfigurationSection
    {
        private const string ConfigPath = "compositeC1Contrib/localization";

        public static LocalizationSection GetSection()
        {
            return ConfigurationManager.GetSection(ConfigPath) as LocalizationSection;
        }

        [ConfigurationProperty("ignoreCase", IsRequired = false, DefaultValue = false)]
        public bool IgnoreCase
        {
            get { return (bool)this["ignoreCase"]; }
            set { this["ignoreCase"] = value; }
        }
    }
}
