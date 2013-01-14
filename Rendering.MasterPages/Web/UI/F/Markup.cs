using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.Localization;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;

namespace CompositeC1Contrib.Web.UI.F
{
    [ParseChildren(false)]
    public class Markup : Control
    {
        protected XElement Content { get; set; }

        public Markup() { }

        public Markup(XElement content)
        {
            Content = content;
        }

        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();

            base.OnInit(e);
        }

        protected override void CreateChildControls()
        {
            if (Content == null)
            {
                string str = null;

                if (Controls.Count > 0)
                {
                    var content = Controls[0] as LiteralControl;
                    if (content != null)
                    {
                        str = content.Text;
                    }
                }

                if (!String.IsNullOrEmpty(str))
                {
                    Controls.Clear();

                    Content = new XElement(Namespaces.Xhtml + "html",
                        new XAttribute(XNamespace.Xmlns + "f", Namespaces.Function10),
                        new XAttribute(XNamespace.Xmlns + "lang", LocalizationXmlConstants.XmlNamespace),
                            new XElement(Namespaces.Xhtml + "head"),
                            new XElement(Namespaces.Xhtml + "body", XElement.Parse(str)));
                }
            }

            if (Content != null)
            {
                var helper = new PageRendererHelper();
                var mapper = (IXElementToControlMapper)helper.FunctionContext.XEmbedableMapper;
                var doc = helper.RenderDocument(Content);

                var body = PageRendererHelper.GetDocumentPart(doc, "body");
                if (body == null)
                {
                    body = new XElement(Namespaces.Xhtml + "body");
                }

                ContentFilterFacade.FilterContent(body, this.ID);

                addNodesAsControls(body.Nodes(), this, mapper);

                if (Page.Header != null)
                {
                    var head = PageRendererHelper.GetDocumentPart(doc, "head");
                    if (head == null)
                    {
                        head = new XElement(Namespaces.Xhtml + "head");
                    }

                    ContentFilterFacade.FilterContent(head, this.ID);

                    addNodesAsControls(head.Nodes(), Page.Header, mapper);
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
