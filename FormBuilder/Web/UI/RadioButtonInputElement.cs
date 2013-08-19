using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class RadioButtonInputElement : IInputElementHandler
    {
        public string ElementName
        {
            get { return "radio"; }
        }

        public IHtmlString GetHtmlString(FormField field, IDictionary<string, object> htmlAttributes)
        {
            var sb = new StringBuilder();

            var value = field.Value;
            var strLabel = field.Label == null ? field.Name : field.Label.Label;
            var strPlaceholder = strLabel;
            var fieldId = FormRenderer.GetFieldId(field);

            if (field.DataSource != null && field.DataSource.Any())
            {
                var ix = 0;

                foreach (var item in field.DataSource)
                {
                    sb.Append("<label class=\"radio\">");

                    sb.AppendFormat("<input type=\"radio\" name=\"{1}\" id=\"{2}\" value=\"{3}\" title=\"{0}\" {4} {5}/> {6}",
                        HttpUtility.HtmlAttributeEncode(item.StringLabel),
                        HttpUtility.HtmlAttributeEncode(field.Name),
                        HttpUtility.HtmlAttributeEncode(fieldId + "_" + ix++),
                        HttpUtility.HtmlAttributeEncode(item.Key),
                        (value == null ? String.Empty : FormRenderer.WriteChecked(FormRenderer.IsEqual(value, item.Key), "checked")),
                        FormRenderer.WriteClass(htmlAttributes),
                        HttpUtility.HtmlEncode(item.StringLabel));

                    sb.Append("</label>");

                    if (item.HtmlLabel != null)
                    {
                        sb.AppendFormat("<div class=\"label-rich\">{0}</div>", item.HtmlLabel);
                    }
                }
            }

            return new HtmlString(sb.ToString());
        }
    }
}
