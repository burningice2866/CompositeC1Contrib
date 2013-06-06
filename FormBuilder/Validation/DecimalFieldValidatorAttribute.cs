using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class DecimalFieldValidatorAttribute : FormValidationAttribute
    {
        public DecimalFieldValidatorAttribute(string message) : base(message) { }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    var s = form.SubmittedValues[prop.Name];
                    var i = 0m;

                    if (!decimal.TryParse(s, out i))
                    {
                        return !form.IsRequired(prop);
                    }

                    return true;
                }
            };
        }
    }
}
