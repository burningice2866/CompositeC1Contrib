using System.Collections.Generic;
using System.Text;
using System.Web;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class PasswordInputElement : IInputElementHandler
    {
        public string ElementName
        {
            get { return "textbox"; }
        }

        public IHtmlString GetHtmlString(FormField field, IDictionary<string, object> htmlAttributes)
        {
            var sb = new StringBuilder();

            var value = field.Value;
            var s = "<input type=\"{0}\" name=\"{1}\" id=\"{2}\" value=\"{3}\" title=\"{4}\" placeholder=\"{4}\" {5} />";

            sb.AppendFormat(s,
                "password",
                HttpUtility.HtmlAttributeEncode(field.Name),
                HttpUtility.HtmlAttributeEncode(field.Id),
                value == null ? "" : HttpUtility.HtmlAttributeEncode(value.ToString()),
                HttpUtility.HtmlAttributeEncode(field.PlaceholderText),
                FormRenderer.WriteClass(htmlAttributes));

            return new HtmlString(sb.ToString());
        }
    }
}
