using System.Web;

namespace CompositeC1Contrib.FormBuilder
{
    public class RichTextListItem
    {
        public string Key { get; set; }
        public string StringLabel { get; set; }
        public IHtmlString HtmlLabel { get; set; }

        public RichTextListItem(string key, string stringLabel)
            : this(key, stringLabel, null) { }

        public RichTextListItem(string key, string stringLabel, IHtmlString htmlLabel)
        {
            Key = key;
            StringLabel = stringLabel;
            HtmlLabel = htmlLabel;
        }
    }
}
