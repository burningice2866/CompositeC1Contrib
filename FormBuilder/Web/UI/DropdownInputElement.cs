using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class DropdownInputElement : IInputElementHandler
    {
        public string ElementName
        {
            get { return "selectbox"; }
        }

        public IHtmlString GetHtmlString(FormField field, IDictionary<string, object> htmlAttributes)
        {
            var sb = new StringBuilder();

            var value = field.Value;
            var strLabel = field.Label == null ? field.Name : field.Label.Label;
            var strPlaceholder = strLabel;
            var fieldId = FormRenderer.GetFieldId(field);

            sb.AppendFormat("<select name=\"{0}\" id=\"{1}\" {2}>",
                        HttpUtility.HtmlAttributeEncode(field.Name),
                        HttpUtility.HtmlAttributeEncode(fieldId),
                        FormRenderer.WriteClass(htmlAttributes));

            if (field.DataSource != null && field.DataSource.Any())
            {
                var selectLabel = field.OwningForm.Options.HideLabels ? strLabel : Localization.Widgets_Dropdown_SelectLabel;

                sb.AppendFormat("<option value=\"\" selected=\"selected\" disabled=\"disabled\">{0}</option>", HttpUtility.HtmlEncode(selectLabel));

                foreach (var item in field.DataSource)
                {
                    sb.AppendFormat("<option value=\"{0}\" {1}>{2}</option>",
                        HttpUtility.HtmlAttributeEncode(item.Key),
                        FormRenderer.WriteChecked(item.Key == (value ?? String.Empty).ToString(), "selected"),
                        HttpUtility.HtmlEncode(item.StringLabel));
                }
            }

            sb.Append("</select>");

            return new HtmlString(sb.ToString());
        }
    }
}
