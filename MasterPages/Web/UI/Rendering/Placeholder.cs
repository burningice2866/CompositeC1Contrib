using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    public class Placeholder : C1MarkupControl
    {
        public string Title { get; set; }
        public bool Default { get; set; }

        public bool HasBody
        {
            get
            {
                var el = CreateElementToRender();
                if (el != null)
                {
                    var body = PageRendererHelper.GetDocumentPart(el, "body");

                    if (body != null)
                    {
                        return body.Nodes().Any();
                    }
                }

                return false;
            }
        }

        protected override XElement CreateElementToRender()
        {
            var rq = RequestInfo.Current;
            var contents = rq.IsPreview ? (IEnumerable<IPagePlaceholderContent>)Page.Cache.Get(rq.PreviewKey + "_SelectedContents") : PageManager.GetPlaceholderContent(Document.Id);

            var content = contents.SingleOrDefault(c => c.PlaceHolderId == ID);
            if (content != null)
            {
                if (content.Content.StartsWith("<html"))
                {
                    try
                    {
                        return XhtmlDocument.Parse(content.Content).Root;
                    }
                    catch (ArgumentException) { }
                }
                else
                {
                    return XhtmlDocument.Parse(String.Format("<html xmlns='{0}'><head/><body>{1}</body></html>", Namespaces.Xhtml, content.Content)).Root;
                }
            }

            return null;
        }
    }
}
