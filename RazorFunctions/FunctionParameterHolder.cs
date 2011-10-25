using System;
using System.Web.WebPages;

namespace CompositeC1Contrib.RazorFunctions
{
    public class FunctionParameterHolder
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public FunctionParameterAttribute Attribute { get; set; }

        public FunctionParameterHolder(string name, Type type, FunctionParameterAttribute att)
        {
            Name = name;
            Type = type;
            Attribute = att;
        }

        public void SetValue(WebPageBase webPage, object value)
        {
            webPage.GetType().GetProperty(Name).SetValue(webPage, value, null);
        }
    }
}
