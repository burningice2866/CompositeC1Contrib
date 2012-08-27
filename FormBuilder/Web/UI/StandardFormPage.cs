using System;
using System.Collections.Generic;
using System.Linq;

using Composite.Core.Xml;

using CompositeC1Contrib.FunctionProvider;

using StandardWidgetFunctions = Composite.Functions.StandardWidgetFunctions;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public abstract class StandardFormPage : BaseFormPage<BaseForm>
    {
        [FunctionParameter("Form type", "Vælg hvilken formular der skal vises")]
        public string FormType { get; set; }

        [FunctionParameter("Before", "Text that is shown above the form but disappears after submission", "")]
        public XhtmlDocument Before { get; set; }

        [FunctionParameter("Success", "Text that is shown on successful submission", "")]
        public XhtmlDocument Success { get; set; }

        protected override Type ResolveFormType()
        {
            return Type.GetType(FormType);
        }

        public virtual ParameterWidgets GetParameterWidgets()
        {
            return new ParameterWidgets()
            {
                { () => this.FormType, StandardWidgetFunctions.DropDownList(typeof(StandardFormPage), "GetFormTypes", "Key", "Value", false, false) }
            };
        }

        public static IDictionary<string, string> GetFormTypes()
        {
            var formTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(BaseForm).IsAssignableFrom(t))
                .ToList();

            return formTypes.ToDictionary(t => t.AssemblyQualifiedName, t => t.Name);
        }
    }
}
