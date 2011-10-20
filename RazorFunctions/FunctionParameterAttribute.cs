using System;

namespace CompositeC1Contrib.RazorFunctions
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class FunctionParameterAttribute : Attribute
    {
        public string Label { get; set; }
        public string HelpText { get; set; }
        public object DefaultValue { get; set; }
        public bool HasDefaultValue { get; set; }

        public FunctionParameterAttribute(string label, string helpText)
        {
            Label = label;
            HelpText = helpText;
            HasDefaultValue = false;
        }

        public FunctionParameterAttribute(string label, string helpText, object defaultValue)
            : this(label, helpText)
        {
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }
    }
}
