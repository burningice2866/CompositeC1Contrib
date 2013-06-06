using System.Reflection;
using System.Web.Security;

using CompositeC1Contrib.FormBuilder;
using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class EmailExistsValidatorAttribute : FormValidationAttribute
    {
        public EmailExistsValidatorAttribute(string message) : base(message) { }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            var value = (string)prop.GetValue(form, null);

            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    var users = Membership.FindUsersByEmail(value);

                    return users.Count != 0;
                }
            };
        }
    }
}
