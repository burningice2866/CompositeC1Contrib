using System;
using System.Linq;
using System.Reflection;
using System.Web.Security;

using CompositeC1Contrib.FormBuilder;
using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.Forms.Validation
{
    public class EmailInUseValidatorAttribute : FormValidationAttribute
    {
        public EmailInUseValidatorAttribute(string message) : base(message) { }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            var value = (string)prop.GetValue(form, null);

            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    var currentUser = Membership.GetUser();
                    if (currentUser != null)
                    {
                        if (!String.Equals(value, currentUser.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            return !IsEmailInUse(value);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return !IsEmailInUse(value);
                    }
                }
            };
        }

        public static bool IsEmailInUse(string email)
        {
            return Membership.FindUsersByEmail(email).Cast<MembershipUser>().Where(u => u.IsApproved).Any();
        }
    }
}
