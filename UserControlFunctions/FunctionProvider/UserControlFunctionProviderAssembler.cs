using System;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;
using Microsoft.Practices.ObjectBuilder;

using Composite.Functions.Plugins.FunctionProvider;

namespace CompositeC1Contrib.UserControlFunctions.FunctionProvider
{
    public class UserControlFunctionProviderAssembler : IAssembler<IFunctionProvider, FunctionProviderData>
    {
        IFunctionProvider IAssembler<IFunctionProvider, FunctionProviderData>.Assemble(IBuilderContext context, FunctionProviderData objectConfiguration, IConfigurationSource configurationSource, ConfigurationReflectionCache reflectionCache)
        {
            var data = objectConfiguration as UserControlFunctionProviderData;
            if (data == null)
            {
                throw new ArgumentException("Expected configuration to be of type UserControlFunctionProviderData", "objectConfiguration");
            }

            return new UserControlFunctionProvider(data.Name, data.Directory);
        }
    }
}
