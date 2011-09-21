using System;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.Xml;

namespace CompositeC1Contrib.Web.UI.Rendering
{
    [ParseChildren(false)]
    public class Function : C1MarkupControl
    {
        public string Content { get; set; }

        protected override XElement CreateElementToRender()
        {
            if (Controls.Count > 0)
            {
                var content = Controls[0] as LiteralControl;
                if (content != null)
                {
                    Content = content.Text;
                }
            }

            if (!String.IsNullOrEmpty(Content))
            {
                Controls.Clear();

                var s = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" xmlns:lang=\"http://www.composite.net/ns/localization/1.0\">"+
                    "<head />"+
                    "<body>"+ Content +"</body>"+
                    "</html>";

                return XElement.Parse(s);
            }

            return null;
        }
    }
}