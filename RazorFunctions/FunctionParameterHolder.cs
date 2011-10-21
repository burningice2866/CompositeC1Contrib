using System;
using System.Web.WebPages;

namespace CompositeC1Contrib.RazorFunctions
{
    public class FunctionParameterHolder
    {
        private Action<WebPageBase, object> _setValue;

        public string Name { get; set; }
        public string Label { get; set; }
        public string HelpText { get; set; }
        public object DefaultValue { get; set; }
        public bool HasDefaultValue { get; set; }
        public Type Type { get; set; }

        public FunctionParameterHolder(string name, Type type, Action<WebPageBase, object> setValue, FunctionParameterAttribute att)
        {
            Name = Label = name;
            Type = type;
            HasDefaultValue = false;
            HelpText = String.Empty;

            _setValue = setValue;

            if (att != null)
            {
                Label = att.Label;
                HelpText = att.HelpText;

                HasDefaultValue = att.HasDefaultValue;
                DefaultValue = att.DefaultValue;
            }
        }

        public void SetValue(WebPageBase webPage, object value)
        {
            _setValue(webPage, value);
        }
    }
}
