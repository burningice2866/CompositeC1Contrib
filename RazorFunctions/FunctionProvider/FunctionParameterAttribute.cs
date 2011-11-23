using System;

namespace CompositeC1Contrib.RazorFunctions.FunctionProvider
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FunctionParameterAttribute : Attribute
    {
        public string Label { get; private set; }
        public string HelpText { get; private set; }
        public object DefaultValue { get; private set; }
        public bool HasDefaultValue { get; private set; }

        public string WidgetMarkup { get; set; }

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
