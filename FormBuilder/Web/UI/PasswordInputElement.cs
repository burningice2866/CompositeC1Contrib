using System;
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
            var strLabel = field.Label == null ? field.Name : field.Label.Label;
            var strPlaceholder = strLabel;
            var fieldId = FormRenderer.GetFieldId(field);

            var s = "<input type=\"{0}\" name=\"{1}\" id=\"{2}\" value=\"{3}\" title=\"{4}\" placeholder=\"{4}\" {5} />";

            sb.AppendFormat(s,
                "password",
                HttpUtility.HtmlAttributeEncode(field.Name),
                HttpUtility.HtmlAttributeEncode(fieldId),
                value == null ? "" : HttpUtility.HtmlAttributeEncode(value.ToString()),
                HttpUtility.HtmlAttributeEncode(strPlaceholder),
                FormRenderer.WriteClass(htmlAttributes));

            return new HtmlString(sb.ToString());
        }
    }
}
