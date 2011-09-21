using System.Web.UI;

using Composite.Data.Types;

namespace CompositeC1Contrib.Web.UI
{
    public class BaseCompositeC1Control : Control
    {
        public new CompositeC1Page Page
        {
            get { return ((CompositeC1Page)base.Page); }
        }

        public IPage Document
        {
            get { return Page.Document; }
        }
    }
}
