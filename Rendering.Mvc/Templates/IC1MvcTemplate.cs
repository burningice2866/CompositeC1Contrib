using System;
using System.Web;
using System.Xml.Linq;

using Composite.Data;

namespace CompositeC1Contrib.Rendering.Mvc.Templates
{
    public interface IC1MvcTemplate<out TTemplateModel> : IDisposable
    {
        DataConnection Data { get; }
        SitemapNavigator Sitemap { get; }
        PageNode CurrentPageNode { get; }
        PageNode HomePageNode { get; }
        string Lang { get; }
        TTemplateModel TemplateModel { get; }
        IHtmlString PageTemplateFeature(string featureName);
        IHtmlString Markup(XNode xNode);
    }
}
