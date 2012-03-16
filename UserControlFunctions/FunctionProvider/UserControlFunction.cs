using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using Composite.Functions;

using CompositeC1Contrib.FunctionProvider;

namespace CompositeC1Contrib.UserControlFunctions.FunctionProvider
{
    public class UserControlFunction : FileBasedFunction<UserControlFunction>
    {
        public UserControlFunction(string ns, string name, string description, IDictionary<string, FunctionParameterHolder> parameters, Type returnType, string virtualPath, FileBasedFunctionProvider<UserControlFunction> provider)
            : base(ns, name, description, parameters, returnType, virtualPath, provider)
        {
        }

        public override IEnumerable<ParameterProfile> ParameterProfiles
        {
            get
            {
                var parameters = base.ParameterProfiles.ToList();
                var idParameter = new ParameterProfile("ID", typeof(string), false, new ConstantValueProvider(null), StandardWidgetFunctions.TextBoxWidget, "ID", new HelpDefinition("Enter ID you want to assign to the UserControl"));

                parameters.Insert(0, idParameter);

                return parameters;
            }
        }

        public override object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            var page = HttpContext.Current.Handler as Page;
            if (page != null)
            {
                var control = (UserControl)page.LoadControl(VirtualPath);

                foreach (var param in parameters.AllParameterNames)
                {
                    var value = parameters.GetParameter(param);

                    if (param == "ID")
                    {
                        control.ID = (string)value;
                    }
                    else
                    {
                        Parameters[param].SetValue(control, value);
                    }
                }

                return control;
            }

            return null;
        }
    }
}
