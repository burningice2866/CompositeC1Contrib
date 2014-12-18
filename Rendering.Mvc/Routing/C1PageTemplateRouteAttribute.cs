using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Routing;
using System.Web.Routing;

using Composite;
using Composite.Core.PageTemplates;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1PageTemplateRouteAttribute : RouteFactoryAttribute
    {
        public IEnumerable<Guid> PageTemplateIds { get; set; }

        public C1PageTemplateRouteAttribute(string template)
            : base(template)
        {
            PageTemplateIds = PageTemplateFacade.GetPageTemplates().OfType<MvcPageTemplateDescriptor>().Select(t => t.Id);
        }

        public C1PageTemplateRouteAttribute(string template, params string[] pageTemplates)
            : base(template)
        {
            var guids = new List<Guid>();

            foreach (var pageTemplate in pageTemplates)
            {
                var templateId = Resolve(pageTemplate);

                Verify.ArgumentCondition(templateId != default(Guid), "pageTemplates", String.Format("Template '{0}' not found", pageTemplate));

                guids.Add(templateId);
            }

            PageTemplateIds = guids;
        }

        public C1PageTemplateRouteAttribute(string template, string pageTemplate)
            : base(template)
        {
            var templateId = Resolve(pageTemplate);

            Verify.ArgumentCondition(templateId != default(Guid), "pageTemplate", String.Format("Template '{0}' not found", pageTemplate));

            PageTemplateIds = new[] { templateId };
        }

        public override RouteValueDictionary Constraints
        {
            get
            {
                var constraints = new RouteValueDictionary 
                {
                    {"templates", new C1PageTemplateRouteConstraint(PageTemplateIds)}
                };

                return constraints;
            }
        }

        private static Guid Resolve(string template)
        {
            Guid templateId;
            if (!Guid.TryParse(template, out templateId))
            {
                var c1Template = PageTemplateFacade.GetPageTemplates().OfType<MvcPageTemplateDescriptor>().FirstOrDefault(t => t.Title == template);
                if (c1Template != null)
                {
                    templateId = c1Template.Id;
                }
            }

            return templateId;
        }
    }
}
