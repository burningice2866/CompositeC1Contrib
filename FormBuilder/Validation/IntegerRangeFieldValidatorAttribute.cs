using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class IntegerRangeFieldValidatorAttribute : FormValidationAttribute
    {
        private int _minValue;
        private int _maxValue;

        public IntegerRangeFieldValidatorAttribute(string message, int minValue, int maxValue)
            : base(message)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            return new FormValidationRule(new[] { prop.Name })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    var s = form.SubmittedValues[prop.Name];
                    var i = 0;

                    if (!int.TryParse(s, out i))
                    {
                        return !form.IsRequired(prop);
                    }
                    else
                    {
                        if (i < _minValue)
                        {
                            return false;
                        }

                        if (i > _maxValue)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            };
        }
    }
}
