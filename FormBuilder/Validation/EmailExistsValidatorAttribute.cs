using System.Web.Security;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class EmailExistsValidatorAttribute : FormValidationAttribute
    {
        public EmailExistsValidatorAttribute(string message) : base(message) { }

        public override FormValidationRule CreateRule(FormField field)
        {
            var value = (string)field.Value;

            return new FormValidationRule(new[] { field.Name })
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
