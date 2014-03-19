using System.Configuration;

namespace CompositeC1Contrib.Teasers.Configuration
{
    public class TeasersTemplatePositionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
    }
}