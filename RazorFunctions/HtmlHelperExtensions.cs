using System.Web.WebPages.Html;

namespace CompositeC1Contrib.RazorFunctions
{
    public static class HtmlHelperExtensions
    {
        public static C1HtmlHelper C1(this HtmlHelper helper)
        {
            return new C1HtmlHelper(helper);
        }
    }
}
