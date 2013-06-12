using Composite.Core.Xml;

namespace CompositeC1Contrib.Web
{
    public interface IContentFilter
    {
        void Filter(XhtmlDocument document, string id);
    }
}
