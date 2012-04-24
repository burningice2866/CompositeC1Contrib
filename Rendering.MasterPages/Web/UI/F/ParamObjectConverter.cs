using System;

namespace CompositeC1Contrib.Web.UI.F
{
    public class ParamObjectConverter
    {
        public object Value { get; set; }

        public ParamObjectConverter() : base() { }

        public ParamObjectConverter(string oValue)
            : this()
        {
            this.Value = oValue;
        }

        public override string ToString()
        {
            if (Value == null)
            {
                return String.Empty;
            }
            else
            {
                return Value.ToString();
            }
        }
    }
}