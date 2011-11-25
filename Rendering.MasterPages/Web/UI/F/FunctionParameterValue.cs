using System.ComponentModel;

namespace CompositeC1Contrib.Web.UI.F
{
    [TypeConverter(typeof(FunctionParameterValueConverter))]
    public class FunctionParameterValue
    {
        public object Value { get; set; }

        public FunctionParameterValue(object value)
        {
            Value = value;
        }
    }
}
