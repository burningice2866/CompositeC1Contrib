using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.Xml;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    [ParseChildren(false)]
    public class Function : C1MarkupControl
    {
        protected override void CreateChildControls()
        {
            var content = this.Controls[0] as LiteralControl;
            if (content != null)
            {
                Controls.Clear();

                var doc = new XElement(Namespaces.Xhtml + "html", 
                    new XElement(Namespaces.Xhtml + "head"), 
                    new XElement(Namespaces.Xhtml + "body", XElement.Parse(content.Text)));
                
                elementToRender = doc;
            }

            base.CreateChildControls();
        }
    }
}
