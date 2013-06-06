using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class FileExtensionValidator : FormValidationAttribute
    {
        private string[] _extensions;

        public FileExtensionValidator(string message, string extension)
            : base(message)
        {
            _extensions = new[] { extension };
        }

        public FileExtensionValidator(string message, params string[] extension)
            : base(message)
        {
            _extensions = extension;
        }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            IEnumerable<FormFile> value;

            if (prop.PropertyType == typeof(FormFile))
            {
                value = new[] { (FormFile)prop.GetValue(form, null) };
            }
            else
            {
                value = (IEnumerable<FormFile>)prop.GetValue(form, null);
            }

            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    foreach (var f in value)
                    {
                        var extension = Path.GetExtension(f.FileName);
                        if (!_extensions.Contains(extension))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            };
        }
    }
}
