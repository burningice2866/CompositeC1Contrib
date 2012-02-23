using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Xml.Linq;

using Composite.Core.Types;
using Composite.Core.Xml;
using Composite.Functions;

namespace CompositeC1Contrib.Web.UI.F
{
    [PersistChildren(false)]
    [ParseChildren(true, "Parameters")]
    public class Function : Control
    {
        public string Name { get; set; }

        private ParamCollection _parameters;

        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        [DefaultValue(default(string))]
        [MergableProperty(false)]
        public ParamCollection Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    _parameters = new ParamCollection();
                }

                return _parameters;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            IFunction function;
            if (!FunctionFacade.TryGetFunction(out function, Name))
            {
                throw new ArgumentException("Invalid function name", "name");
            }

            var result = FunctionFacade.Execute<object>(function, parseParameters(), new FunctionContextContainer());

            if (function.ReturnType == typeof(XElement) || function.ReturnType == typeof(XhtmlDocument))
            {
                var element = ValueTypeConverter.Convert<XElement>(result);
                var markup = new Markup(element);

                Controls.Add(markup);
            }
            else if (typeof(Control).IsAssignableFrom(function.ReturnType))
            {
                var control = (Control)result;

                Controls.Add(control);
            }
            else
            {
                var str = result.ToString();

                Controls.Add(new LiteralControl(str));
            }

            base.OnInit(e);
        }

        private IDictionary<string, object> parseParameters()
        {
            var result = new Dictionary<string, object>();

            foreach (Param param in Parameters)
            {
                param.DataBind();

                result.Add(param.Name, param.Value.Value);
            }

            return result;
        }
    }
}