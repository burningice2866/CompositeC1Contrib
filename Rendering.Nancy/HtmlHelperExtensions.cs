using Nancy.ViewEngines.Razor;

namespace CompositeC1Contrib.Rendering.Nancy
{
    public static class HtmlHelperExtensions
    {
        public static C1HtmlHelper<T> C1<T>(this HtmlHelpers<T> helper)
        {
            return new C1HtmlHelper<T>(helper);
        }
    }
}
