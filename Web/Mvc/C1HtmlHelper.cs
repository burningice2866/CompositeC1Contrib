using System.Globalization;
using System.Linq;
using System.Web.Mvc;

using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.Mvc
{
    public static class C1HtmlHelper
    {
        public static MvcHtmlString TemplatePlaceHolder(this HtmlHelper<PageModel> html, string placeHolderId)
        {
            var page = html.ViewData.Model.Document;

            using (var data = new DataConnection(CultureInfo.CurrentCulture))
            {
                var placeholder = data.Get<IPagePlaceholderContent>().SingleOrDefault(p => p.PageId == page.Id && p.PlaceHolderId == placeHolderId);
                if (placeholder != null)
                {
                    var helper = new PageRendererHelper();
                    var doc = helper.RenderDocument(placeholder);

                    var body = PageRendererHelper.GetDocumentPart(doc, "body");
                    var content = PageRendererHelper.CopyWithoutNamespace(body, Namespaces.Xhtml);

                    return new MvcHtmlString(content.ToString());
                }
            }

            return MvcHtmlString.Empty;
        }
    }
}
