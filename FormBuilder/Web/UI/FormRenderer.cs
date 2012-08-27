using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public static class FormRenderer
    {
        public static IHtmlString FieldFor<T>(this T form, Expression<Func<T, object>> field) where T : BaseForm
        {
            var prop = form.GetProperty(field);

            return FieldFor(form, prop);
        }

        public static IHtmlString FieldFor(BaseForm form, PropertyInfo prop)
        {
            return writeRow(form, prop);
        }

        public static IHtmlString Errors(this BaseForm form)
        {
            var sb = new StringBuilder();
            var validationResult = form.Validate();

            if (validationResult.Any())
            {
				sb.Append("<div class=\"error_notification\">");
                sb.Append("<p>Du mangler at udfylde nogle felter:</p>");
                sb.Append("<ul>");

                foreach (var el in validationResult)
                {
                    sb.Append("<li>" + el.ValidationMessage + "</li>");
                }

                sb.Append("</ul>");
                sb.Append("<p>Udfyld venligst felterne og send igen.</p>");
                sb.Append("</div>");
            }

            return new HtmlString(sb.ToString());
        }

        private static IHtmlString writeRow(BaseForm form, PropertyInfo prop)
        {
            var attributes = prop.GetCustomAttributes(true);

            bool required = false;
            var name = prop.Name;
            FieldLabelAttribute label = null;
            var type = InputType.Textbox;

            foreach (var attr in attributes)
            {
                if (attr is RequiredFieldAttribute)
                {
                    required = true;
                }

                var labelAttribute = attr as FieldLabelAttribute;
                if (labelAttribute != null)
                {
                    label = labelAttribute;
                }

                var inputTypeAttribute = attr as InputFieldTypeAttribute;
                if (inputTypeAttribute != null)
                {
                    type = inputTypeAttribute.InputType;
                }
            }

            var sb = new StringBuilder();

			var validationResult = form.Validate();
			sb.AppendFormat("<div class=\"control-group control-{0} {1} {2}\">", getFieldName(type), writeErrorClass(name, validationResult), required ? "required" : String.Empty);
            writeTitle(label, name, required, sb);

			sb.Append("<div class=\"controls\">");

            writeField(type, name, form, prop, sb);

            sb.Append("</div></div>");

            return new HtmlString(sb.ToString());
        }

        private static void writeField(InputType type, string name, BaseForm form, PropertyInfo prop, StringBuilder sb)
        {
            var value = prop.GetValue(form, null);

            switch (type)
            {
                case InputType.Checkbox:

                    var check = (bool)value ? "checked=\"checked\"" : "";

                    sb.AppendFormat("<input type=\"checkbox\" name=\"{0}\" id=\"{0}\" value=\"on\" {1} />", name, check);

                    break;

                case InputType.Dropdown:

                    IDictionary<string, string> options = null;

                    var datasourceAttribute = prop.GetCustomAttributes(typeof(DataSourceAttribute), true).FirstOrDefault() as DataSourceAttribute;
                    if (datasourceAttribute != null)
                    {
                        var ds = datasourceAttribute.GetData(form);

                        var dict = ds as IDictionary<string, string>;
                        if (dict != null)
                        {
                            options = dict;
                        }
                    }

                    sb.AppendFormat("<select name=\"{0}\" id=\"{0}\">", name);

                    if (options != null)
                    {
                        foreach (var kvp in options)
                        {
                            sb.AppendFormat("<option value=\"{0}\" {1}>{2}</option>", kvp.Key, writeChecked(kvp.Key, value.ToString(), "selected"), kvp.Value);
                        }
                    }

                    sb.Append("</select>");

                    break;

                case InputType.TextArea:

                    sb.AppendFormat("<textarea id=\"{0}\" name=\"{0}\" rows=\"5\" cols=\"40\">{1}</textarea>",
                        name,
                        value);

                    break;

                case InputType.Textbox:
                case InputType.Password:

                    sb.AppendFormat("<input type=\"{0}\" id=\"{1}\" name=\"{1}\" value=\"{2}\" />",
                        type == InputType.Textbox ? "text" : "password",
                        name,
                        value);

                    break;
            }
        }

        private static string getFieldName(InputType type)
        {
            switch (type)
            {
                case InputType.Checkbox: return "checkbox";
                case InputType.Dropdown: return "selectbox";
                case InputType.TextArea: return "textarea";

				case InputType.Password:
                case InputType.Textbox: return "textbox";

				default: return "textbox";
            }
        }

        private static string writeChecked(string option, string value, string attr)
        {
            if (value == option)
            {
                return String.Format("{0}=\"{0}\"", attr);
            }

            return String.Empty;
        }

        private static string writeTitle(FieldLabelAttribute label, string name, bool required, StringBuilder sb)
        {
            var title = label == null ? name : label.Label;

            sb.AppendFormat("<label class=\"control-label\" for=\"{0}\">", name);

            if (required)
            {
                sb.Append("<span class=\"required\">*</span>");
            }

            if (label != null && !String.IsNullOrEmpty(label.Link))
            {
                sb.AppendFormat("<a href=\"{0}\" title=\"{1}\" {2}>{1}</a>", label.Link, title, label.OpenLinkInNewWindow ? "target=\"_blank\"" : String.Empty);
            }
            else
            {
                sb.Append(title);
            }

            sb.Append(":");
            sb.Append("</label>");

            return sb.ToString();
        }

        private static string writeErrorClass(string name, IEnumerable<FormValidationRule> validationResult)
        {
            if (validationResult.Any(el => el.AffectedFormIds.Contains(name)))
            {
                return "error";
            }

            return String.Empty;
        }

        private static T getAttribute<T>(object[] attributes) where T : Attribute
        {
            foreach (var attr in attributes)
            {
                if (attr is T)
                {
                    return (T)attr;
                }
            }

            return null;
        }
    }
}
