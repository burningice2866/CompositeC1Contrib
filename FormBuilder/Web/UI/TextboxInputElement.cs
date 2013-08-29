﻿using System;
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
            var s = "<input type=\"{0}\" name=\"{1}\" id=\"{2}\" value=\"{3}\" title=\"{4}\" placeholder=\"{5}\" {6} />";

            sb.AppendFormat(s,
                evaluateTextboxType(field),
                HttpUtility.HtmlAttributeEncode(field.Name),
                HttpUtility.HtmlAttributeEncode(field.Id),
                field.Value == null ? String.Empty : HttpUtility.HtmlAttributeEncode(getValue(field)),
                HttpUtility.HtmlAttributeEncode(field.Label.Label),
                HttpUtility.HtmlAttributeEncode(field.PlaceholderText),
                FormRenderer.WriteClass(htmlAttributes));

            return new HtmlString(sb.ToString());
        }

        private static string getValue(FormField field) 
        {
            if (field.ValueType == typeof(DateTime))
            {
                return ((DateTime)field.Value).ToString("yyyy-MM-dd");
            }

            if (field.ValueType == typeof(DateTime?))
            {
                var dt = (DateTime?)field.Value;
                if (dt.HasValue)
                {
                    return dt.Value.ToString("yyyy-MM-dd");
                }
            }

            return field.Value.ToString();
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