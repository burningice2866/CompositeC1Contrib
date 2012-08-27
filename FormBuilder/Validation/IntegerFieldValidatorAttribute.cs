using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class IntegerFieldValidatorAttribute : FormValidationAttribute
    {
        public IntegerFieldValidatorAttribute(string message) : base(message) { }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    var s = form.GetFormValue(prop.Name);
                    var i = 0;

                    if (!int.TryParse(s, out i))
                    {
                        return !form.IsRequired(prop);
                    }

                    return true;
                }
            };
        }
    }
}
