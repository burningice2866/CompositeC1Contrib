using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using Composite.Core.Instrumentation;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data.Types;
using Composite.Functions;

namespace CompositeC1Contrib
{
    public class PageRendererHelper
    {
        private static readonly MethodInfo _normalizeXhtmlDocument = typeof(PageRenderer).GetMethod("NormalizeXhtmlDocument", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _resolveRelativePaths = typeof(PageRenderer).GetMethod("ResolveRelativePaths", BindingFlags.Static | BindingFlags.NonPublic);

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

                var xDoc = new XhtmlDocument(doc);

                _normalizeXhtmlDocument.Invoke(null, new[] { xDoc });
                _resolveRelativePaths.Invoke(null, new[] { xDoc });
            }

            return doc;
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
    }
}
