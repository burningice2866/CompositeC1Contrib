using System;
using System.Configuration;
using System.Linq;

namespace CompositeC1Contrib.Teasers.Configuration
{
    public class TeasersTemplateElement : ConfigurationElement
    {
        [ConfigurationProperty("positions", IsRequired = true)]
        public TeasersTemplatePositionCollection Positions
        {
            get { return (TeasersTemplatePositionCollection)this["positions"]; }
            set { this["positions"] = value; }
        }

        [ConfigurationProperty("guid", IsRequired = true)]
        public Guid Guid
        {
            get { return (Guid)this["guid"]; }
            set { this["guid"] = value; }
        }

        [ConfigurationProperty("defaultPosition")]
        public string DefaultPosition
        {
            get { return (string)this["defaultPosition"]; }
            set { this["defaultPosition"] = value; }
        }

        protected override void PostDeserialize()
        {
            if (Positions.Cast<TeasersTemplatePositionElement>().All(e => e.Name != DefaultPosition))
            {
                throw new ConfigurationErrorsException("Unrecognized default position");
            }

            base.PostDeserialize();
        }
    }
}