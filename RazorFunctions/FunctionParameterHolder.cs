using System;
using System.Web.WebPages;

namespace CompositeC1Contrib.RazorFunctions
{
    public class FunctionParameterHolder
    {
        private Action<WebPageBase, object> _setValue;

        public string Name { get; set; }
        public Type Type { get; set; }
        public FunctionParameterAttribute Attribute { get; set; }

        public FunctionParameterHolder(string name, Type type, Action<WebPageBase, object> setValue, FunctionParameterAttribute att)
        {
            Name = name;
            Type = type;
            Attribute = att;

            _setValue = setValue;
        }

        public void SetValue(WebPageBase webPage, object value)
        {
            _setValue(webPage, value);
        }
    }
}
