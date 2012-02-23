using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;

using Composite.C1Console.Security;
using Composite.Functions;

using CompositeC1Contrib.FunctionProvider;
using CompositeC1Contrib.UserControlFunctions.Security;

namespace CompositeC1Contrib.UserControlFunctions.FunctionProvider
{
    public class UserControlFunction : IFunction
    {
        string _relativeFilePath;
        IDictionary<string, FunctionParameterHolder> _parameters;

        private string _ns;
        public string Namespace
        {
            get { return _ns; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
        }

        public Type ReturnType
        {
            get { return typeof(UserControl); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
        }

        public EntityToken EntityToken
        {
            get { return new UserControlFunctionEntityToken(typeof(UserControlFunctionProvider).Name, String.Join(".", Namespace, Name)); }
        }

        public IEnumerable<ParameterProfile> ParameterProfiles
        {
            get
            {
                yield return new ParameterProfile("ID", typeof(string), false, new ConstantValueProvider(null), StandardWidgetFunctions.TextBoxWidget, "ID", new HelpDefinition("Enter ID you want to assign to the UserControl"));

                if (_parameters != null)
                {
                    foreach (var param in _parameters.Values)
                    {
                        BaseValueProvider defaultValueProvider = new NoValueValueProvider();
                        WidgetFunctionProvider widgetProvider = null;
                        var label = param.Name;
                        var isRequired = true;
                        var helpText = String.Empty;

                        if (param.Attribute != null)
                        {
                            label = param.Attribute.Label;
                            helpText = param.Attribute.HelpText;

                            isRequired = !param.Attribute.HasDefaultValue;
                            if (!isRequired)
                            {
                                defaultValueProvider = new ConstantValueProvider(param.Attribute.DefaultValue);
                            }

                            if (!String.IsNullOrEmpty(param.Attribute.WidgetMarkup))
                            {
                                var xElement = XElement.Parse(param.Attribute.WidgetMarkup);

                                widgetProvider = new WidgetFunctionProvider(xElement);
                            }
                        }

                        if (widgetProvider == null)
                        {
                            widgetProvider = StandardWidgetFunctions.GetDefaultWidgetFunctionProviderByType(param.Type);
                        }

                        yield return new ParameterProfile(param.Name, param.Type, isRequired, defaultValueProvider, widgetProvider, label, new HelpDefinition(helpText));
                    }
                }
            }
        }

        public UserControlFunction(string ns, string name, string description, IDictionary<string, FunctionParameterHolder> parameters, string relativeFilePath)
        {
            _ns = ns;
            _name = name;
            _description = description;
            _parameters = parameters;
            _relativeFilePath = relativeFilePath;
        }

        public object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            var page = HttpContext.Current.Handler as Page;
            if (page != null)
            {
                var control = (UserControl)page.LoadControl(_relativeFilePath);

                foreach (var param in parameters.AllParameterNames)
                {
                    var value = parameters.GetParameter(param);

                    if (param == "ID")
                    {
                        control.ID = (string)value;
                    }
                    else
                    {
                        _parameters[param].SetValue(control, value);
                    }
                }

                return control;
            }

            return null;
        }
    }
}
