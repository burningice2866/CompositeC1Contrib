using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.WebClient.Renderings.Template;
using Composite.Data;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public class C1HtmlHelper
    {
        private readonly HtmlHelper _helper;

        public C1HtmlHelper(HtmlHelper helper)
        {
            _helper = helper;
        }

        public MvcHtmlString TemplatePlaceHolder(string placeHolderId)
        {
            var page = PageRenderer.CurrentPage;
            var placeholderContent = PageManager.GetPlaceholderContent(page.Id).SingleOrDefault(p => p.PlaceHolderId == placeHolderId);

            return placeholderContent != null ? MvcHtmlString.Create(placeholderContent.Content) : MvcHtmlString.Empty;
        }

        public IHtmlString GetPageTemplateFeature(string featureName)
        {
            var root = PageTemplateFeatureFacade.GetPageTemplateFeature(featureName).Root;

            return _helper.Raw(root.ToString());
        }

        public IHtmlString Markup(XNode xNode)
        {
            return xNode == null ? null : _helper.Raw(xNode.ToString());
        }
    }
}
