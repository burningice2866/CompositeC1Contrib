using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CompositeC1Contrib.FormBuilder
{
    public class RichTextListItem
    {
        public RichTextListItem(string key, string stringLabel)
            : this(key, stringLabel, null)
        {
        }

        public RichTextListItem(string key, string stringLabel, IHtmlString htmlLabel)
        {
            Key = key;
            StringLabel = stringLabel;
            HtmlLabel = htmlLabel;
        }

        public string Key { get; set; }
        public string StringLabel { get; set; }
        public IHtmlString HtmlLabel { get; set; }
    }
}
