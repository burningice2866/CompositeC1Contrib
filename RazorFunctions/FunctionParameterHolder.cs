using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
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

        public FunctionParameterHolder(string name, Type type, FunctionParameterAttribute att, Action<WebPageBase, object> setValue)
        {
            Name = name;
            Label = att.Label;
            HelpText = att.HelpText;
            DefaultValue = att.DefaultValue;
            HasDefaultValue = att.HasDefaultValue;
            Type = type;

            _setValue = setValue;
        }

        public void SetValue(WebPageBase webPage, object value)
        {
            _setValue(webPage, value);
        }
    }
}
