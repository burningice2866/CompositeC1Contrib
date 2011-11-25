using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.WebClient.Renderings.Page;

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

                    var s = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" xmlns:lang=\"http://www.composite.net/ns/localization/1.0\">" +
                        "<head />" +
                        "<body>" + str + "</body>" +
                        "</html>";

                    Content = XElement.Parse(s);
                }
            }

            if (Content != null)
            {
                var helper = new PageRendererHelper();
                var mapper = (IXElementToControlMapper)helper.FunctionContext.XEmbedableMapper;

                var doc = helper.RenderDocument(Content);
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
