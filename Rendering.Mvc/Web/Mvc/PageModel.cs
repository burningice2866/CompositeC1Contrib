using System.Web.Mvc;

using Composite.Data.Types;

namespace CompositeC1Contrib.Web.Mvc
{
    public class PageModel : ViewPage<System.String>
    {
        public IPage Document { get; private set; }

        public PageModel(IPage page)
        {
            Document = page;
        }
    }
}
