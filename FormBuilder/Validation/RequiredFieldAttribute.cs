using System;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class RequiredFieldAttribute : FormValidationAttribute
    {
        public RequiredFieldAttribute(string message) : base(message) { }

        public override FormValidationRule CreateRule(FormField field)
        {
            var value = field.Value;

            return new FormValidationRule(new[] { field.Name })
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
