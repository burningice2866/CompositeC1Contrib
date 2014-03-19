using System.Configuration;

namespace CompositeC1Contrib.Teasers.Configuration
{
	public class TeasersPositionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("label", IsRequired = true)]
        public string Label
        {
			get { return (string)this["label"]; }
			set { this["label"] = value; }
        }
    }
}