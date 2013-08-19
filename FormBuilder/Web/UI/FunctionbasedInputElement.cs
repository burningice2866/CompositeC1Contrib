using System;
using System.Collections.Generic;
using System.Web;

using Composite.Core.Xml;
using Composite.Functions;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class FunctionbasedInputElement : IInputElementHandler
    {
        private string _c1FunctionName;

        public string ElementName
        {
            get { return String.Empty; }
        }

        public FunctionbasedInputElement(string functionName)
        {
            _c1FunctionName = functionName;
        }

        public IHtmlString GetHtmlString(FormField field, IDictionary<string, object> htmlAttributes)
        {
            var function = FunctionFacade.GetFunction(_c1FunctionName);
            if (function == null)
            {
                throw new InvalidOperationException("C1 function " + _c1FunctionName + " not recognized");
            }

            var paramenters = new Dictionary<string, object> 
            {
                { "Name", field.Name }
            };

            var result = FunctionFacade.Execute<XhtmlDocument>(function, paramenters);

            return new HtmlString(result.ToString());
        }
    }
}
