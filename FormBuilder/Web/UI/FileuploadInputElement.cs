using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class FileuploadInputElement : IInputElementHandler
    {
        public string ElementName
        {
            get { return "file"; }
        }

        public IHtmlString GetHtmlString(FormField field, IDictionary<string, object> htmlAttributes)
        {
            var sb = new StringBuilder();

            var value = field.Value;
            var strLabel = field.Label == null ? field.Name : field.Label.Label;
            var strPlaceholder = strLabel;

            sb.AppendFormat("<input type=\"file\" name=\"{0}\" id=\"{1}\" ",
                        HttpUtility.HtmlAttributeEncode(field.Name),
                        HttpUtility.HtmlAttributeEncode(field.Id));

            if (field.ValueType == typeof(IEnumerable<FormFile>))
            {
                sb.Append("multiple=\"multiple\" ");
            }

            var fileMimeTypeValidatorAttr = field.Attributes.OfType<FileMimeTypeValidatorAttribute>().SingleOrDefault();
            if (fileMimeTypeValidatorAttr != null)
            {
                sb.Append("accept=\"" + String.Join(",", fileMimeTypeValidatorAttr.MimeTypes) + "\" ");
            }

            sb.Append("/>");

            return new HtmlString(sb.ToString());
        }
    }
}
