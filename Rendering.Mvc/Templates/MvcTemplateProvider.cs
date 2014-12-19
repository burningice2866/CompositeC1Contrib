using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.Core.PageTemplates;

using CompositeC1Contrib.Rendering.Mvc.Templates;

namespace CompositeC1Contrib.Rendering.Mvc
{
    [ConfigurationElementType(typeof(MvcTemplateProviderData))]
    public class MvcTemplateProvider : IPageTemplateProvider
    {
        public string ProviderName { get; private set; }
        public string TemplatesFolderPath { get; private set; }

        public string AddNewTemplateLabel
        {
            get { return "Can't be added through the console";  }
        }

        public Type AddNewTemplateWorkflow
        {
            get { return null; }
        }

        public MvcTemplateProvider(string providerName, string templatesFolderPath)
        {
            ProviderName = providerName;
            TemplatesFolderPath = templatesFolderPath;

            ViewEngines.Engines.Add(new C1RazorTemplateViewEngine(templatesFolderPath));
        }

        public IPageRenderer BuildPageRenderer(Guid templateId)
        {
            return new MvcPageRenderer();
        }

        public void FlushTemplates()
        {
            
        }

        public IEnumerable<PageTemplateDescriptor> GetPageTemplates()
        {
            return GlobalConfiguration.Current.Templates;
        }

        public IEnumerable<ElementAction> GetRootActions()
        {
            return Enumerable.Empty<ElementAction>();
        }
    }
}
