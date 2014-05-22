using System.Configuration;

namespace CompositeC1Contrib.Teasers.Configuration
{
	public class TeasersSection : ConfigurationSection
    {
        [ConfigurationProperty("designs", IsRequired = false)]
        public TeasersDesignCollection Designs
        {
            get { return (TeasersDesignCollection)this["designs"]; }
            set { this["designs"] = value; }
        }

        [ConfigurationProperty("positions", IsRequired = true)]
		public TeasersPositionCollection Positions
        {
			get { return (TeasersPositionCollection)this["positions"]; }
			set { this["positions"] = value; }
        }

        [ConfigurationProperty("templates", IsRequired = true)]
        public TeasersTemplateCollection Templates
        {
            get { return (TeasersTemplateCollection)this["templates"]; }
            set { this["templates"] = value; }
        }

		public static TeasersSection GetSection()
        {
            return ConfigurationManager.GetSection("compositeC1Contrib/teasers") as TeasersSection;
        }
    }
}
