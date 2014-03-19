using Composite.AspNet.Razor;

using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.Web.UI
{
    public abstract class BaseTeaserFunction<T> : RazorFunction where T : ITeaser
    {
        public T Teaser { get; set; }
    }
}
