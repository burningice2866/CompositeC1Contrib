using System;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace CompositeC1Contrib.ECommerce
{
    public static class NameValueCollectionExtensions
    {
        public static string ToXml(this NameValueCollection values)
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
    }
}
