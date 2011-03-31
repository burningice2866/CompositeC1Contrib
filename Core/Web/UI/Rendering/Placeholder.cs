using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    public class Placeholder : C1MarkupControl
    {
        public string Title { get; set; }
        public bool Default { get; set; }

        protected override void CreateChildControls()
        {
            var rq = RequestInfo.Current;

            var contents = rq.IsPreview ? (IEnumerable<IPagePlaceholderContent>)Page.Cache.Get(rq.PreviewKey + "_SelectedContents") : PageManager.GetPlaceholderContent(Document.Id);
            var content = contents.Single(c => c.PlaceHolderId == ID);

            elementToRender = XElement.Parse(content.Content);

            base.CreateChildControls();
        }
    }
}
