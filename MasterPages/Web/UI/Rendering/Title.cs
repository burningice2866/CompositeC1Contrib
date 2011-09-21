using System.Web.UI;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    public class Title : BaseCompositeC1Control
    {
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(Document.Title);
        }
    }
}
