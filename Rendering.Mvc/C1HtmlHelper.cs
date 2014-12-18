using System.Linq;
using System.Web.Mvc;

using Composite.Core.WebClient.Renderings.Page;
using Composite.Data;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public static class C1HtmlHelper
    {
        public static MvcHtmlString TemplatePlaceHolder(this HtmlHelper html, string placeHolderId)
        {
            return TemplatePlaceHolder(placeHolderId);
        }

        public static MvcHtmlString TemplatePlaceHolder<T>(this HtmlHelper<T> htmlHelper, string placeHolderId)
        {
            return TemplatePlaceHolder(placeHolderId);
        }

        private static MvcHtmlString TemplatePlaceHolder(string placeHolderId)
        {
            var page = PageRenderer.CurrentPage;

            var placeholderContent = PageManager.GetPlaceholderContent(page.Id).SingleOrDefault(p => p.PlaceHolderId == placeHolderId);
            if (placeholderContent != null)
            {
                return MvcHtmlString.Create(placeholderContent.Content);
            }

            return MvcHtmlString.Empty;
        }
    }
}
