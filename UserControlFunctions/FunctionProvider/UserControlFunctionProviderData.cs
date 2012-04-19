using System.Configuration;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;

using Composite.Functions.Plugins.FunctionProvider;

namespace CompositeC1Contrib.UserControlFunctions.FunctionProvider
{
    [Assembler(typeof(UserControlFunctionProviderAssembler))]
    public class UserControlFunctionProviderData : FunctionProviderData
    {
        [ConfigurationProperty("directory", IsRequired = false, DefaultValue = "~/App_Data/UserControl")]
        public string Directory
        {
            get { return (string)base["directory"]; }
            set { base["directory"] = value; }
        }
    }
}
