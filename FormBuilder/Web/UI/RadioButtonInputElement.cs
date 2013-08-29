﻿using System;
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

            if (field.DataSource != null && field.DataSource.Any())
            {
                var ix = 0;
                var value = field.Value;

                foreach (var item in field.DataSource)
                {
                    sb.Append("<label class=\"radio\">");

                    sb.AppendFormat("<input type=\"radio\" name=\"{1}\" id=\"{2}\" value=\"{3}\" title=\"{0}\" {4} {5}/> {6}",
                        HttpUtility.HtmlAttributeEncode(item.Value),
                        HttpUtility.HtmlAttributeEncode(field.Name),
                        HttpUtility.HtmlAttributeEncode(field.Id + "_" + ix++),
                        HttpUtility.HtmlAttributeEncode(item.Key),
                        (value == null ? String.Empty : FormRenderer.WriteChecked(FormRenderer.IsEqual(value, item.Key), "checked")),
                        FormRenderer.WriteClass(htmlAttributes),
                        HttpUtility.HtmlEncode(item.Value));

                    sb.Append("</label>");
                }
            }

            return new HtmlString(sb.ToString());
        }
    }
}