using System.Globalization;
using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class DecimalRangeFieldValidatorAttribute : FormValidationAttribute
    {
        private decimal _minValue;
        private decimal _maxValue;

        public DecimalRangeFieldValidatorAttribute(string message, string minValue)
            : base(message)
        {
            _minValue = decimal.Parse(minValue, CultureInfo.InvariantCulture.NumberFormat);
            _maxValue = decimal.MaxValue;
        }

        public DecimalRangeFieldValidatorAttribute(string message, string minValue, string maxValue)
            : base(message)
        {
            _minValue = decimal.Parse(minValue, CultureInfo.InvariantCulture.NumberFormat);
            _maxValue = decimal.Parse(maxValue, CultureInfo.InvariantCulture.NumberFormat);
        }

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
