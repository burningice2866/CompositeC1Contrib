using System;
using System.Text.RegularExpressions;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class EmailFieldValidatorAttribute : FormValidationAttribute
    {
        private static readonly Regex _emailValidationRegex = new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public EmailFieldValidatorAttribute(string message) : base(message) { }

        public override FormValidationRule CreateRule(FormField field)
        {
            var value = (string)field.Value;

            return new FormValidationRule(new[] { field.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        return !field.IsRequired;
                    }

                    return !Validate(value);
                }
            };
        }

        public static bool Validate(string email)
        {
            return !_emailValidationRegex.IsMatch(email);
        }
    }
}
