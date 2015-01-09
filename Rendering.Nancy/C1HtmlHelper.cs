using System.Xml.Linq;

using Composite.Core.WebClient.Renderings.Template;

using Nancy.ViewEngines.Razor;

namespace CompositeC1Contrib.Rendering.Nancy
{
    public class C1HtmlHelper<T>
    {
        private readonly HtmlHelpers<T> _helper;

        public C1HtmlHelper(HtmlHelpers<T> helper)
        {
            _helper = helper;
        }

        public IHtmlString GetPageTemplateFeature(string featureName)
        {
            var root = PageTemplateFeatureFacade.GetPageTemplateFeature(featureName).Root;

            return root == null ? NonEncodedHtmlString.Empty : _helper.Raw(root.ToString());
        }

        public IHtmlString Markup(XNode xNode)
        {
            return xNode == null ? null : _helper.Raw(xNode.ToString());
        }
    }
}
