using System.Xml.Linq;

namespace CompositeC1Contrib.Web
{
    public interface IContentFilter
    {
        void Filter(XElement document, string id);
    }
}
