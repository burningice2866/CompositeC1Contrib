using System.Configuration;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;

using Composite.Core.PageTemplates.Plugins;

namespace CompositeC1Contrib.Rendering.Mvc
{
    [Assembler(typeof(MvcTemplateProviderAssembler))]
    public class MvcTemplateProviderData : PageTemplateProviderData
    {
        [ConfigurationProperty("directory", IsRequired = false, DefaultValue = "~/App_Data/PageTemplates/Mvc")]
        public string Directory
        {
            get { return (string)base["directory"]; }
            set { base["directory"] = value; }
        }
    }
}
