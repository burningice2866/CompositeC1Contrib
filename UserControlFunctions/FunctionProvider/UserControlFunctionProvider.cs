using System;
using System.Web.UI;

using CompositeC1Contrib.FunctionProvider;

namespace CompositeC1Contrib.UserControlFunctions.FunctionProvider
{
    public class UserControlFunctionProvider : FileBasedFunctionProvider<UserControlFunction>
    {
        protected override string Folder
        {
            get { return "UserControl"; }
        }

        protected override string FileExtension
        {
            get { return "ascx"; }
        }

        protected override Type BaseType
        {
            get { return typeof(UserControl); }
        }

        protected override Type GetReturnType(object obj)
        {
            return typeof(UserControl);
        }

        protected override object InstantiateFile(string virtualPath)
        {
            using (new PopulateUserControlParametersContext())
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
