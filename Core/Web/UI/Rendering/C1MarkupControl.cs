using System;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    public abstract class C1MarkupControl : BaseCompositeC1Control
    {
        protected abstract XElement CreateElementToRender();

        protected override void CreateChildControls()
        {
            var elementToRender = CreateElementToRender();
            if (elementToRender != null)
            {
                var helper = new PageRendererHelper();

                var doc = helper.RenderDocument(elementToRender);
                var body = PageRendererHelper.GetDocumentPart(doc, "body");
                var control = body.AsAspNetControl((IXElementToControlMapper)helper.FunctionContext.XEmbedableMapper);

                Controls.Add(control);

                if (Page.Header != null)
                {
                    var head = PageRendererHelper.GetDocumentPart(doc, "head");
                    if (head != null)
                    {
                        Page.Header.Controls.Add(new LiteralControl(String.Concat(head.Nodes())));
                    }
                }
            }

            base.CreateChildControls();
        }
    }
}
