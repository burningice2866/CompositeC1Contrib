using System;
using System.Collections.Generic;

using Composite.C1Console.Security;
using Composite.Core.Xml;
using Composite.Functions;

namespace CompositeC1Contrib.FormBuilder.FunctionProviders
{
    public class FormFunction : IFunction
    {
        private BaseForm _form;

        public EntityToken EntityToken
        {
            get { return new FormFunctionEntityToken(typeof(FormBuilderFunctionProvider).Name, String.Join(".", Namespace, Name)); }
        }

        public string Namespace
        {
            get { return "Forms"; }
        }

        public string Name
        {
            get { return _form.GetType().Name; }
        }

        public string Description
        {
            get { return ""; }
        }

        public Type ReturnType
        {
            get { return typeof(XhtmlDocument); }
        }

        public IEnumerable<ParameterProfile> ParameterProfiles
        {
            get
            {
                var list = new List<ParameterProfile>();

                list.Add(new ParameterProfile("IntroText", typeof(XhtmlDocument), false, new ConstantValueProvider(String.Empty), StandardWidgetFunctions.GetDefaultWidgetFunctionProviderByType(typeof(XhtmlDocument)), "Intro text", new HelpDefinition("Intro text")));
                list.Add(new ParameterProfile("SuccessResponse", typeof(XhtmlDocument), false, new ConstantValueProvider(String.Empty), StandardWidgetFunctions.GetDefaultWidgetFunctionProviderByType(typeof(XhtmlDocument)), "Success response", new HelpDefinition("Success response")));

                return list;
            }
        }

        public FormFunction(BaseForm form)
        {
            _form = form;
        }

        public object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            return new XhtmlDocument();   
        }        
    }
}
