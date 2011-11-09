using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.WebClient.Renderings.Page;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    public abstract class C1MarkupControl : Control
    {
        protected abstract XElement CreateElementToRender();

        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();

            base.OnInit(e);
        }

        protected override void CreateChildControls()
        {
            var elementToRender = CreateElementToRender();
            if (elementToRender != null)
            {
                var helper = new PageRendererHelper();
                var mapper = (IXElementToControlMapper)helper.FunctionContext.XEmbedableMapper;

                var doc = helper.RenderDocument(elementToRender);
                var body = PageRendererHelper.GetDocumentPart(doc, "body");

                if (body != null)
                {
                    addNodesAsControls(body.Nodes(), this, mapper);

                    if (Page.Header != null)
                    {
                        var head = PageRendererHelper.GetDocumentPart(doc, "head");
                        if (head != null)
                        {
                            addNodesAsControls(head.Nodes(), Page.Header, mapper);
                        }
                    }
                }
            }

            base.CreateChildControls();
        }

        private static void addNodesAsControls(IEnumerable<XNode> nodes, Control parent, IXElementToControlMapper mapper)
        {
            foreach (var node in nodes)
            {
                var c = node.AsAspNetControl(mapper);
                parent.Controls.Add(c);
            }
        }
    }
}
