using System;
using System.Web.UI;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using CompositeC1Contrib.FunctionProvider;

namespace CompositeC1Contrib.UserControlFunctions.FunctionProvider
{
    [ConfigurationElementType(typeof(UserControlFunctionProviderData))]
    public class UserControlFunctionProvider : FileBasedFunctionProvider<UserControlFunction>
    {
        protected override string FileExtension
        {
            get { return "ascx"; }
        }

        protected override Type BaseType
        {
            get { return typeof(UserControl); }
        }

        public UserControlFunctionProvider(string name, string folder) : base(name, folder) { }

        protected override Type GetReturnType(object obj)
        {
            return typeof(UserControl);
        }

        protected override object InstantiateFile(string virtualPath)
        {
            using (new NoHttpContext())
            {
                var p = new Page();

                return (UserControl)p.LoadControl(virtualPath);
            }
        }

        protected override bool HandleChange(string path)
        {
            return path.EndsWith(".ascx") || path.EndsWith(".ascx.cs");
        }
    }
}
