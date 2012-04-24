using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.Localization;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Web.UI.F;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    public class Placeholder : Markup
    {
        public string Title { get; set; }
        public bool Default { get; set; }

        public bool HasBody
        {
            get
            {
                EnsureChildControls();

                if (Content != null)
                {
                    var body = PageRendererHelper.GetDocumentPart(Content, "body");

                    if (body != null)
                    {
                        return body.Nodes().Any();
                    }
                }

                return false;
            }
        }

        protected override void CreateChildControls()
        {
            var rq = RequestInfo.Current;
            var contents = rq.IsPreview ? (IEnumerable<IPagePlaceholderContent>)Page.Cache.Get(rq.PreviewKey + "_SelectedContents") : PageManager.GetPlaceholderContent(PageRenderer.CurrentPage.Id);

            var content = contents.SingleOrDefault(c => c.PlaceHolderId == ID);
            if (content == null)
            {
                Content = new XElement(Namespaces.Xhtml + "html",
                        new XAttribute(XNamespace.Xmlns + "f", Namespaces.Function10),
                        new XAttribute(XNamespace.Xmlns + "lang", LocalizationXmlConstants.XmlNamespace),
                            new XElement(Namespaces.Xhtml + "head"),
                            new XElement(Namespaces.Xhtml + "body"));
            }
            else
            {
                if (content.Content.StartsWith("<html"))
                {
                    try
                    {
                        Content = XhtmlDocument.Parse(content.Content).Root;
                    }
                    catch (ArgumentException) { }
                }
                else
                {
                    Content = new XElement(Namespaces.Xhtml + "html",
                       new XAttribute(XNamespace.Xmlns + "f", Namespaces.Function10),
                           new XElement(Namespaces.Xhtml + "head"),
                           new XElement(Namespaces.Xhtml + "body", XElement.Parse(content.Content)));
                }
            }

            base.CreateChildControls();
        }
    }
}
