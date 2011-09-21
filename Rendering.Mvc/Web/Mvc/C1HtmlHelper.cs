using System.Linq;
using System.Web.Mvc;

using Composite.Core.Xml;
using Composite.Data;

using CompositeC1Contrib.Web.UI;

namespace CompositeC1Contrib.Web.Mvc
{
    public static class C1HtmlHelper
    {
        public static MvcHtmlString TemplatePlaceHolder(this HtmlHelper<PageModel> html, string placeHolderId)
        {
            var page = html.ViewData.Model.Document;

            var content = PageManager.GetPlaceholderContent(page.Id).SingleOrDefault(p => p.PlaceHolderId == placeHolderId);
            if (content != null)
            {
                var helper = new PageRendererHelper();
                var doc = helper.RenderDocument(content);

                var body = PageRendererHelper.GetDocumentPart(doc, "body");
                var element = PageRendererHelper.CopyWithoutNamespace(body, Namespaces.Xhtml);

                return MvcHtmlString.Create(element.ToString());
            }

            return MvcHtmlString.Empty;
        }
    }
}
