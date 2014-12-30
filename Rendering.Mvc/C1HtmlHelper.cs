using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

using Composite.Core.WebClient.Renderings.Template;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class C1HtmlHelper
    {
        private readonly HtmlHelper _helper;

        public C1HtmlHelper(HtmlHelper helper)
        {
            _helper = helper;
        }

        public IHtmlString GetPageTemplateFeature(string featureName)
        {
            var root = PageTemplateFeatureFacade.GetPageTemplateFeature(featureName).Root;
            
            return root == null ? MvcHtmlString.Empty : _helper.Raw(root.ToString());
        }

        public IHtmlString Markup(XNode xNode)
        {
            return xNode == null ? null : _helper.Raw(xNode.ToString());
        }
    }
}
