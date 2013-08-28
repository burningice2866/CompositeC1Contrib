using System.Collections.Generic;
using System.Text;
using System.Web;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class TextAreaInputElement : IInputElementHandler
    {
        public string ElementName
        {
            get { return "textarea"; }
        }

        public IHtmlString GetHtmlString(FormField field, IDictionary<string, object> htmlAttributes)
        {
            var sb = new StringBuilder();

            var value = field.Value;
            var textarea = "<textarea name=\"{0}\" id=\"{1}\" rows=\"5\" cols=\"40\" title=\"{2}\" placeholder=\"{2}\" {3}>{4}</textarea>";

            sb.AppendFormat(textarea,
                HttpUtility.HtmlAttributeEncode(field.Name),
                HttpUtility.HtmlAttributeEncode(field.Id),
                HttpUtility.HtmlAttributeEncode(field.PlaceholderText),
                FormRenderer.WriteClass(htmlAttributes),
                HttpUtility.HtmlEncode(value));

            return new HtmlString(sb.ToString());
        }
    }
}
