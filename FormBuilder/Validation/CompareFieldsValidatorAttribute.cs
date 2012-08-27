using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class CompareFieldsValidatorAttribute : FormValidationAttribute
    {
        private string _fieldToCompare;
        private CompareOperator _op;

        public CompareFieldsValidatorAttribute(string message, string fieldToCompare, CompareOperator op)
            : base(message)
        {
            _fieldToCompare = fieldToCompare;
            _op = op;
        }

        public override FormValidationRule CreateRule(PropertyInfo prop, BaseForm form)
        {
            var value = prop.GetValue(form, null);
            var valueToCompare = form.GetProperty(_fieldToCompare).GetValue(form, null);

            return new FormValidationRule(new[] { prop.Name, _fieldToCompare })
            {
                ValidationMessage = Message,
                Rule = () =>
                {
                    switch (_op)
                    {
                        case CompareOperator.Equal: return value.Equals(valueToCompare);
                        case CompareOperator.NotEqual: return !value.Equals(valueToCompare);
                    }

                    return true;
                }
            };
        }
    }
}
