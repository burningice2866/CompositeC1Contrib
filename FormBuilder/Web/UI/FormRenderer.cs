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
            var includeLabel = showLabel(field.InputType, field);
            var validationResult = field.OwningForm.ValidationResult;
            var fieldId = getFieldId(field);

            sb.AppendFormat("<div id=\"form-field-{0}\" class=\"control-group control-{1} {2} {3} \"", field.Name, getFieldName(field.InputType), WriteErrorClass(field.Name, validationResult), field.IsRequired ? "required" : String.Empty);

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

            if (includeLabel)
            {
                writeLabel(field.InputType, field.Label, fieldId, field.Name, field.IsRequired, field.OwningForm.Options.HideLabels, sb);
            }
            else
            {
                writePropertyHeading(field.Label, field.Name, field.IsRequired, sb);
            }

            sb.Append("<div class=\"controls\">");

            writeField(field.InputType, field.Name, field.Help, field.Label, field.IsRequired, field.OwningForm.Options, field, sb, htmlAttributes);

            sb.Append("</div></div>");

            return new HtmlString(sb.ToString());
        }


        private static bool showLabel(InputType type, FormField field)
        {
            if (field.ValueType == typeof(bool))
            {
                return true;
            }

            if (type == InputType.Checkbox || type == InputType.RadioButton)
            {
                return false;
            }

            return true;
        }

        private static string getFieldId(FormField field)
        {
            return (field.OwningForm.Name + field.Name).Replace(".", "_");
        }

        private static void writeField(InputType type, string name, string help, FieldLabelAttribute attrLabel, bool required, FormOptions options, FormField field, StringBuilder sb, IDictionary<string, object> htmlAttributes)
        {
            var value = field.Value;
            var strLabel = attrLabel == null ? name : attrLabel.Label;
            var fieldId = getFieldId(field);

            if (!String.IsNullOrWhiteSpace(help))
            {
                sb.Append("<div class=\"input-append\">");
            }

            switch (type)
            {
                case InputType.Checkbox:

                    if (field.ValueType == typeof(bool))
                    {
                        var check = (bool)value ? "checked=\"checked\"" : "";

                        sb.AppendFormat("<input type=\"checkbox\" name=\"{0}\" id=\"{1}\" value=\"on\" title=\"{2}\" {3} {4} />",
                            HttpUtility.HtmlAttributeEncode(name),
                            HttpUtility.HtmlAttributeEncode(fieldId),
                            HttpUtility.HtmlAttributeEncode(strLabel),
                            check,
                            writeClass(htmlAttributes));
                    }
                    else if (field.ValueType == typeof(IEnumerable<string>))
                    {
                        var checkboxListOptions = field.DataSource;
                        if (checkboxListOptions != null)
                        {
                            var ix = 0;
                            var list = value == null ? Enumerable.Empty<string>() : (IEnumerable<string>)value;

                            foreach (var item in checkboxListOptions)
                            {
                                sb.Append("<label class=\"checkbox\">");

                                sb.AppendFormat("<input type=\"checkbox\" name=\"{1}\" id=\"{2}\" value=\"{3}\" title=\"{0}\" {4} {5}/> {6} ",
                                    HttpUtility.HtmlAttributeEncode(item.StringLabel),
                                    HttpUtility.HtmlAttributeEncode(name),
                                    HttpUtility.HtmlAttributeEncode(fieldId + "_" + ix++),
                                    HttpUtility.HtmlAttributeEncode(item.Key),
                                    writeChecked(list.Contains(item.Key), "checked"),
                                    writeClass(htmlAttributes),
                                    HttpUtility.HtmlEncode(item.StringLabel));

                                sb.Append("</label>");

                                if (item.HtmlLabel != null)
                                {
                                    sb.AppendFormat("<div class=\"label-rich\">{0}</div>", item.HtmlLabel);
                                }
                            }
                        }
                    }

                    break;

                case InputType.RadioButton:

                    if (field.DataSource != null && field.DataSource.Any())
                    {
                        var ix = 0;

                        foreach (var item in field.DataSource)
                        {
                            sb.Append("<label class=\"radio\">");

                            sb.AppendFormat("<input type=\"radio\" name=\"{1}\" id=\"{2}\" value=\"{3}\" title=\"{0}\" {4} {5}/> {6}",
                                HttpUtility.HtmlAttributeEncode(item.StringLabel),
                                HttpUtility.HtmlAttributeEncode(name),
                                HttpUtility.HtmlAttributeEncode(fieldId + "_" + ix++),
                                HttpUtility.HtmlAttributeEncode(item.Key),
                                (value == null ? String.Empty : writeChecked(isEqual(value, item.Key), "checked")),
                                writeClass(htmlAttributes),
                                HttpUtility.HtmlEncode(item.StringLabel));

                            sb.Append("</label>");

                            if (item.HtmlLabel != null)
                            {
                                sb.AppendFormat("<div class=\"label-rich\">{0}</div>", item.HtmlLabel);
                            }
                        }
                    }

                    break;

                case InputType.Dropdown:

                    sb.AppendFormat("<select name=\"{0}\" id=\"{1}\" {2}>",
                        HttpUtility.HtmlAttributeEncode(name),
                        HttpUtility.HtmlAttributeEncode(fieldId),
                        writeClass(htmlAttributes));

                    if (field.DataSource != null && field.DataSource.Any())
                    {
                        var selectLabel = options.HideLabels ? strLabel : Localization.Widgets_Dropdown_SelectLabel;

                        sb.AppendFormat("<option value=\"\" selected=\"selected\" disabled=\"disabled\">{0}</option>", HttpUtility.HtmlEncode(selectLabel));

                        foreach (var item in field.DataSource)
                        {
                            sb.AppendFormat("<option value=\"{0}\" {1}>{2}</option>",
                                HttpUtility.HtmlAttributeEncode(item.Key),
                                writeChecked(item.Key == (value ?? String.Empty).ToString(), "selected"),
                                HttpUtility.HtmlEncode(item.StringLabel));
                        }
                    }

                    sb.Append("</select>");

                    break;

                case InputType.TextArea:
                    var textarea = "<textarea name=\"{0}\" id=\"{1}\" rows=\"5\" cols=\"40\" title=\"{2}\" placeholder=\"{2}\" {3}>{4}</textarea>";

                    sb.AppendFormat(textarea,
                        HttpUtility.HtmlAttributeEncode(name),
                        HttpUtility.HtmlAttributeEncode(fieldId),
                        HttpUtility.HtmlAttributeEncode(strLabel),
                        writeClass(htmlAttributes),
                        HttpUtility.HtmlEncode(value));

                    break;

                case InputType.Textbox:
                case InputType.Password:

                    var s = "<input type=\"{0}\" name=\"{1}\" id=\"{2}\" value=\"{3}\" title=\"{4}\" placeholder=\"{4}\" {5} />";

                    sb.AppendFormat(s,
                        type == InputType.Textbox ? evaluateTextboxType(field) : "password",
                        HttpUtility.HtmlAttributeEncode(name),
                        HttpUtility.HtmlAttributeEncode(fieldId),
                        value == null ? "" : HttpUtility.HtmlAttributeEncode(value.ToString()),
                        HttpUtility.HtmlAttributeEncode(strLabel),
                        writeClass(htmlAttributes));

                    break;

                case InputType.Fileupload:

                    sb.AppendFormat("<input type=\"file\" name=\"{0}\" id=\"{1}\" ",
                        HttpUtility.HtmlAttributeEncode(name),
                        HttpUtility.HtmlAttributeEncode(fieldId));

                    if (field.ValueType == typeof(IEnumerable<FormFile>))
                    {
                        sb.Append("multiple=\"multiple\" ");
                    }

                    var fileMimeTypeValidatorAttr = field.Attributes.OfType<FileMimeTypeValidatorAttribute>().SingleOrDefault();
                    if (fileMimeTypeValidatorAttr != null)
                    {
                        sb.Append("accept=" + String.Join(",", fileMimeTypeValidatorAttr.MimeTypes) +" ");
                    }

                    sb.Append("/>");

                    break;
            }

            if (!String.IsNullOrWhiteSpace(help))
            {
                sb.Append("<div class=\"info-block\">");
                sb.Append("<span class=\"add-on info-icon\">i</span>");
                sb.AppendFormat("<div class=\"info-msg\">{0}</div>", HttpUtility.HtmlEncode(help));
                sb.Append("</div>");
                sb.Append("</div>");
            }
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

        private static string getFieldName(InputType type)
        {
            switch (type)
            {
                case InputType.Checkbox: return "checkbox";
                case InputType.Dropdown: return "selectbox";
                case InputType.TextArea: return "textarea";
                case InputType.Fileupload: return "file";
                case InputType.RadioButton: return "radio";

                case InputType.Password:
                case InputType.Textbox: return "textbox";

                default: return "textbox";
            }
        }

        private static string writeClass(IDictionary<string, object> htmlAttributes)
        {
            if (htmlAttributes.ContainsKey("class"))
            {
                return "class=\"" + htmlAttributes["class"] + "\"";
            }

            return String.Empty;
        }

        private static string writeChecked(bool write, string attr)
        {
            if (write)
            {
                return String.Format("{0}=\"{0}\"", attr);
            }

            return String.Empty;
        }

        private static bool isEqual(object obj, string value)
        {
            if (obj is bool)
            {
                return bool.Parse(value) == (bool)obj;
            }

            return obj.ToString() == value;
        }

        private static string writeLabel(InputType type, FieldLabelAttribute label, string fieldId, string name, bool required, bool hide, StringBuilder sb)
        {
            if (type == InputType.Fileupload)
            {
                hide = false;
            }

            sb.AppendFormat("<label class=\"control-label {0}\" for=\"{1}\">", hide ? "hide-text " : String.Empty, fieldId);

            writeLabelContent(required, label, name, sb);

            sb.Append(":");
            sb.Append("</label>");

            return sb.ToString();
        }

        private static string writePropertyHeading(FieldLabelAttribute label, string name, bool required, StringBuilder sb)
        {
            sb.Append("<p class=\"control-label\">");

            writeLabelContent(required, label, name, sb);

            sb.Append("</p>");

            return sb.ToString();
        }

        private static void writeLabelContent(bool required, FieldLabelAttribute label, string name, StringBuilder sb)
        {
            var title = label == null ? name : label.Label;

            if (required)
            {
                sb.Append("<span class=\"required\">*</span>");
            }

            if (label != null && !String.IsNullOrEmpty(label.Link))
            {
                sb.AppendFormat("<a href=\"{0}\" title=\"{1}\" {2}>{3}</a>",
                    HttpUtility.HtmlAttributeEncode(label.Link),
                    HttpUtility.HtmlAttributeEncode(title),
                    label.OpenLinkInNewWindow ? "target=\"_blank\"" : String.Empty,
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
