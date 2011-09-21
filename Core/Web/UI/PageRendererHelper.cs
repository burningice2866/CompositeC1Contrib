using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.Instrumentation;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data.Types;
using Composite.Functions;

namespace CompositeC1Contrib.Web.UI
{
    public class PageRendererHelper
    {
        public FunctionContextContainer FunctionContext { get; private set; } 

        public PageRendererHelper()
        {
            FunctionContext = PageRenderer.GetPageRenderFunctionContextContainer();
        }

        public XElement RenderDocument(IPagePlaceholderContent content)
        {
            var doc = XElement.Parse(content.Content);
            return RenderDocument(doc);
        }

        public XElement RenderDocument(XElement doc)
        {
            using (Profiler.Measure("Executing C1 functions"))
            {
                PageRenderer.ExecuteEmbeddedFunctions(doc, FunctionContext);

                try
                {
                    var xDoc = new XhtmlDocument(doc);

                    PageRenderer.NormalizeXhtmlDocument(xDoc);
                    PageRenderer.ResolveRelativePaths(xDoc);

                    return xDoc.Root;
                }
                catch (ArgumentException)
                {
                    return doc;
                }
            }
        }

        public static XElement GetDocumentPart(XElement doc, string part)
        {
            return doc.Descendants().SingleOrDefault(el => el.Name.LocalName == part);
        }

        public static XNode CopyWithoutNamespace(XNode node, XNamespace @namespace)
        {
            var element = node as XElement;
            if (element != null)
            {
                var copy = element.Name.Namespace == @namespace ? new XElement(element.Name.LocalName) : new XElement(element.Name);

                var attributesCleaned = element.Attributes().Where(f => f.Name.Namespace == @namespace).Select(f => new XAttribute(f.Name.LocalName, f.Value));
                var attributesRaw = element.Attributes().Where(f => f.Name.Namespace != @namespace && f.Name.LocalName != "xmlns");

                copy.Add(attributesCleaned);
                copy.Add(attributesRaw);

                foreach (var child in element.Nodes())
                {
                    copy.Add(CopyWithoutNamespace(child, @namespace));
                }

                return copy;
            }            

            return node;
        }

        public void AddNodesAsControls(IEnumerable<XNode> nodes, Control parent)
        {
            var mapper = (IXElementToControlMapper)FunctionContext.XEmbedableMapper;

            foreach (var node in nodes)
            {
                var c = node.AsAspNetControl(mapper);
                parent.Controls.Add(c);
            }
        }
    }
}
