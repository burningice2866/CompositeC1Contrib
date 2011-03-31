using System.Web.UI;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    public class Description : BaseCompositeC1Control
    {
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(Document.Description);
        }
    }
}
