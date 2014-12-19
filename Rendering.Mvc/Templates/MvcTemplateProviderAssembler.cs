using System;
using System.Configuration;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;
using Microsoft.Practices.ObjectBuilder;

using Composite.Core.IO;
using Composite.Core.PageTemplates;
using Composite.Core.PageTemplates.Plugins;

namespace CompositeC1Contrib.Rendering.Mvc.Templates
{
    public class MvcTemplateProviderAssembler : IAssembler<IPageTemplateProvider, PageTemplateProviderData>
    {
        public IPageTemplateProvider Assemble(IBuilderContext context, PageTemplateProviderData objectConfiguration, IConfigurationSource configurationSource, ConfigurationReflectionCache reflectionCache)
        {
            var data = objectConfiguration as MvcTemplateProviderData;
            if (data == null)
            {
                throw new ArgumentException("Expected configuration to be of type " + typeof(MvcTemplateProviderData).Name, "objectConfiguration");
            }

            var path = PathUtil.Resolve(data.Directory);
            if (!C1Directory.Exists(path))
            {
                throw new ConfigurationErrorsException(String.Format("Folder '{0}' does not exists", path), objectConfiguration.ElementInformation.Source, objectConfiguration.ElementInformation.LineNumber);
            }

            return new MvcTemplateProvider(data.Name, data.Directory);
        }
    }
}
