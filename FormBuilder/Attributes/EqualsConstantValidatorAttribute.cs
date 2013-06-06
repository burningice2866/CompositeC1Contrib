using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class EqualsConstantValidatorAttribute : FormValidationAttribute
    {
        object _constant;

        public EqualsConstantValidatorAttribute(string message, object constant)
            : base(message)
        {
            _constant = constant;
        }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            var value = prop.GetValue(form, null);

            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    return (value == _constant);
                }
            };
        }
    }
}
