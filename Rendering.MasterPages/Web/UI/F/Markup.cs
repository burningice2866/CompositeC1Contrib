using System;
using System.Collections.Generic;
using System.Linq;
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
        private static IList<IContentFilter> _contentFilters;

        protected XElement Content { get; set; }

        static Markup()
        {
            _contentFilters = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IContentFilter).IsAssignableFrom(t) && !t.IsInterface)
                .Select(t => (IContentFilter)Activator.CreateInstance(t))
                .ToList();
        }

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

                filterContent(body);

                addNodesAsControls(body.Nodes(), this, mapper);

                if (Page.Header != null)
                {
                    var head = PageRendererHelper.GetDocumentPart(doc, "head");
                    if (head == null)
                    {
                        head = new XElement(Namespaces.Xhtml + "head");
                    }

                    filterContent(head);

                    addNodesAsControls(head.Nodes(), Page.Header, mapper);
                }

            }

            base.CreateChildControls();
        }

        private void filterContent(XElement doc)
        {
            foreach (var filter in _contentFilters)
            {
                filter.Filter(doc, this.ID);
            }
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
