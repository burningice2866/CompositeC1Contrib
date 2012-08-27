using System;
using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class MinFieldLengthAttribute : FormValidationAttribute
    {
        private int _length;

        public MinFieldLengthAttribute(string message, int length)
            : base(message)
        {
            _length = length;
        }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            var value = (string)prop.GetValue(form, null);

            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = String.Format(Message, _length),
                Rule = () =>
                {
                    if (String.IsNullOrEmpty(value) && !form.IsRequired(prop))
                    {
                        return true;
                    }
                    else
                    {
                        return value.Length >= _length;
                    }
                }
            };
        }
    }
}
