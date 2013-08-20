using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public static class FormRenderer
    {
        public static IHtmlString FieldFor(FormField field)
        {
            return writeRow(field, new Dictionary<string, object>());
        }

        public static IHtmlString NameFor(FormField field)
        {
            return new HtmlString(field.Name);
        }

        public static IHtmlString WriteErrors(FormModel model)
        {
            var sb = new StringBuilder();
            var validationResult = model.ValidationResult;

            if (validationResult.Any())
            {
                sb.Append("<div class=\"error_notification\">");

                if (!String.IsNullOrEmpty(Localization.Validation_ErrorNotificationTop))
                {
                    sb.Append("<p>" + HttpUtility.HtmlEncode(Localization.Validation_ErrorNotificationTop) + "</p>");
                }

                sb.Append("<ul>");

                foreach (var el in validationResult)
                {
                    sb.Append("<li>" + el.ValidationMessage + "</li>");
                }

                sb.Append("</ul>");

                if (!String.IsNullOrEmpty(Localization.Validation_ErrorNotificationBottom))
                {
                    sb.Append("<p>" + HttpUtility.HtmlEncode(Localization.Validation_ErrorNotificationBottom) + "</p>");
                }

                sb.Append("</div>");
            }

            return new HtmlString(sb.ToString());
        }

        private static IHtmlString writeRow(FormField field, IDictionary<string, object> htmlAttributes)
        {
            var sb = new StringBuilder();
            var includeLabel = showLabel(field);
            var validationResult = field.OwningForm.ValidationResult;

            sb.AppendFormat("<div id=\"form-field-{0}\" class=\"control-group control-{1} {2} {3} \"", field.Name, field.InputTypeHandler.ElementName, WriteErrorClass(field.Name, validationResult), field.IsRequired ? "required" : String.Empty);

            if (field.DependencyAttributes.Any())
            {
                var dependencyObj = new StringBuilder();
                var attrs = field.DependencyAttributes.ToList();

                dependencyObj.Append("[ ");

                for (int i = 0; i < attrs.Count; i++)
                {
                    var dependencyAttribute = attrs[i];
                    var values = dependencyAttribute.RequiredFieldValues();

                    dependencyObj.Append("{ &quot;field&quot;: &quot;" + dependencyAttribute.ReadFromFieldName + "&quot;, &quot;value&quot;:");

                    dependencyObj.Append("[ ");

                    for (int j = 0; j < values.Length; j++)
                    {
                        dependencyObj.Append("&quot;" + values[j] + "&quot;");

                        if (j < (values.Length - 1))
                        {
                            dependencyObj.Append(",");
                        }
                    }

                    dependencyObj.Append(" ]");

                    dependencyObj.Append(" }");

                    if (i < (attrs.Count - 1))
                    {
                        dependencyObj.Append(", ");
                    }
                }

                dependencyObj.Append(" ]");

                sb.AppendFormat(" data-dependency=\"{0}\"", dependencyObj);
            }

            sb.Append(">");

            if (!(field.InputTypeHandler is CheckboxInputElement && field.ValueType == typeof(bool)))
            {
                if (includeLabel)
                {
                    writeLabel(field, field.Id, field.OwningForm.Options.HideLabels, sb);
                }
                else
                {
                    writePropertyHeading(field, sb);
                }
            }

            sb.Append("<div class=\"controls\">");

            if (field.InputTypeHandler is CheckboxInputElement && field.ValueType == typeof(bool))
            {
                sb.Append("<label class=\"checkbox\">");
            }

            writeField(field, sb, htmlAttributes);

            if (field.InputTypeHandler is CheckboxInputElement && field.ValueType == typeof(bool))
            {
                writeLabelContent(field, sb);

                sb.Append("</label>");
            }

            sb.Append("</div></div>");

            return new HtmlString(sb.ToString());
        }


        private static bool showLabel(FormField field)
        {
            if (field.ValueType == typeof(bool))
            {
                return true;
            }

            if (field.InputTypeHandler is CheckboxInputElement || field.InputTypeHandler is RadioButtonInputElement)
            {
                return false;
            }

            return true;
        }

        private static void writeField(FormField field, StringBuilder sb, IDictionary<string, object> htmlAttributes)
        {
            var value = field.Value;
            var strLabel = field.Label == null ? field.Name : field.Label.Label;
            var strPlaceholder = strLabel;

            if (!field.OwningForm.Options.HideLabels)
            {
                var placeholderAttr = field.Attributes.OfType<PlaceholderTextAttribute>().SingleOrDefault();
                if (placeholderAttr != null)
                {
                    strPlaceholder = placeholderAttr.Text;
                }
            }

            if (!String.IsNullOrWhiteSpace(field.Help))
            {
                sb.Append("<div class=\"input-append\">");
            }

            var str = field.InputTypeHandler.GetHtmlString(field, htmlAttributes);
            sb.Append(str);            

            if (!String.IsNullOrWhiteSpace(field.Help))
            {
                sb.Append("<div class=\"info-block\">");
                sb.Append("<span class=\"add-on info-icon\">i</span>");
                sb.AppendFormat("<div class=\"info-msg\">{0}</div>", HttpUtility.HtmlEncode(field.Help));
                sb.Append("</div>");
                sb.Append("</div>");
            }
        }

        public static string WriteClass(IDictionary<string, object> htmlAttributes)
        {
            if (htmlAttributes.ContainsKey("class"))
            {
                return "class=\"" + htmlAttributes["class"] + "\"";
            }

            return String.Empty;
        }

        public static string WriteChecked(bool write, string attr)
        {
            if (write)
            {
                return String.Format("{0}=\"{0}\"", attr);
            }

            return String.Empty;
        }

        public static bool IsEqual(object obj, string value)
        {
            if (obj is bool)
            {
                return bool.Parse(value) == (bool)obj;
            }

            return obj.ToString() == value;
        }

        private static void writeLabel(FormField field, string fieldId, bool hide, StringBuilder sb)
        {
            if (field.InputTypeHandler is FileuploadInputElement)
            {
                hide = false;
            }

            sb.AppendFormat("<label class=\"control-label {0}\" for=\"{1}\">", hide ? "hide-text " : String.Empty, fieldId);

            writeLabelContent(field, sb);

            sb.Append(":");
            sb.Append("</label>");
        }

        private static string writePropertyHeading(FormField field, StringBuilder sb)
        {
            sb.Append("<p class=\"control-label\">");

            writeLabelContent(field, sb);

            sb.Append("</p>");

            return sb.ToString();
        }

        private static void writeLabelContent(FormField field, StringBuilder sb)
        {
            var title = field.Label == null ? field.Name : field.Label.Label;

            if (field.IsRequired)
            {
                sb.Append("<span class=\"required\">*</span>");
            }

            if (field.Label != null && !String.IsNullOrEmpty(field.Label.Link))
            {
                sb.AppendFormat("<a href=\"{0}\" title=\"{1}\" {2}>{3}</a>",
                    HttpUtility.HtmlAttributeEncode(field.Label.Link),
                    HttpUtility.HtmlAttributeEncode(title),
                    field.Label.OpenLinkInNewWindow ? "target=\"_blank\"" : String.Empty,
                    HttpUtility.HtmlEncode(title));
            }
            else
            {
                sb.Append(HttpUtility.HtmlEncode(title));
            }
        }

        public static string WriteErrorClass(string name, IEnumerable<FormValidationRule> validationResult)
        {
            if (validationResult.Any(el => el.AffectedFormIds.Contains(name)))
            {
                return "error";
            }

            return String.Empty;
        }
    }
}
