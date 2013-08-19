using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class TextboxInputElement : IInputElementHandler
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
                evaluateTextboxType(field),
                HttpUtility.HtmlAttributeEncode(field.Name),
                HttpUtility.HtmlAttributeEncode(fieldId),
                value == null ? "" : HttpUtility.HtmlAttributeEncode(value.ToString()),
                HttpUtility.HtmlAttributeEncode(strPlaceholder),
                FormRenderer.WriteClass(htmlAttributes));

            return new HtmlString(sb.ToString());
        }

        private static string evaluateTextboxType(FormField field)
        {
            var type = Nullable.GetUnderlyingType(field.ValueType) ?? field.ValueType;

            if (type == typeof(DateTime))
            {
                return "date";
            }

            if (type == typeof(int))
            {
                return "number";
            }

            if (type == typeof(string))
            {
                if (field.ValidationAttributes.Any(f => f is EmailFieldValidatorAttribute))
                {
                    return "email";
                }
            }

            return "text";
        }
    }
}
