using System.Web.UI;

using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    public class Description : Control
    {
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(PageRenderer.CurrentPage.Description);
        }
    }
}
