using System;
using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class RequiredFieldAttribute : FormValidationAttribute
    {
        public RequiredFieldAttribute(string message) : base(message) { }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            var value = prop.GetValue(form, null);

            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    if (value is string)
                    {
                        return !String.IsNullOrWhiteSpace((string)value);
                    }
                    else if (value is bool)
                    {
                        return (bool)value;
                    }
                    else if (value is int)
                    {
                        return (int)value > 0;
                    }
                    else if (value is int?)
                    {
                        return ((int?)value).HasValue;
                    }
                    else
                    {
                        return value != null;
                    };
                }
            };
        }
    }
}
