using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Routing;
using System.Web.Routing;

using Composite;
using Composite.Core.PageTemplates;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Data;
using Composite.Data.Types;
using CompositeC1Contrib.Rendering.Mvc.Templates;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1RouteAttribute : RouteFactoryAttribute
    {
        public string Page { get; set; }
        public string PageTemplates { get; set; }
        public string PageType { get; set; }
        public string Suffix { get; set; }
        public RenderingReason Reason { get; set; }

        public C1RouteAttribute() : base("{*url}") { }

        public override RouteValueDictionary Constraints
        {
            get
            {
                var constraints = new RouteValueDictionary();

                if (!String.IsNullOrEmpty(Page))
                {
                    Guid pageId;
                    if (Guid.TryParse(Page, out pageId))
                    {
                        constraints.Add("pageId", new C1PageRouteConstraint(pageId));
                    }
                }

                if (!String.IsNullOrEmpty(PageTemplates))
                {
                    var templates = PageTemplates.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var templateIds = new List<Guid>();

                    foreach (var template in templates)
                    {
                        var templateId = ResolveTemplate(template);

                        Verify.ArgumentCondition(templateId.HasValue, "PageTemplates", String.Format("Template '{0}' not found", template));

                        templateIds.Add(templateId.Value);
                    }

                    if (templateIds.Any())
                    {
                        constraints.Add("templates", new C1PageTemplateRouteConstraint(templateIds));
                    }
                }

                if (!String.IsNullOrEmpty(PageType))
                {
                    var typeId = ResolveType(PageType);

                    Verify.ArgumentCondition(typeId.HasValue, "PageType", String.Format("Pagetype '{0}' not found", PageType));

                    constraints.Add("type", new C1PageTypeRouteConstraint(typeId.Value));
                }

                if (!String.IsNullOrEmpty(Suffix))
                {
                    constraints.Add("pathinfo", new C1PathInfoRouteConstraint(Suffix));
                }

                if (Reason != RenderingReason.Undefined)
                {
                    constraints.Add("reason", new C1RenderingReasonConstraint(Reason));
                }

                return constraints;
            }
        }

        private static Guid? ResolveType(string type)
        {
            Guid typeId;
            if (Guid.TryParse(type, out typeId))
            {
                return typeId;
            }

            using (var data = new DataConnection())
            {
                var pageType = data.Get<IPageType>().FirstOrDefault(p => p.Name.Equals(type, StringComparison.OrdinalIgnoreCase));

                return pageType == null ? (Guid?)null : pageType.Id;
            }
        }

        private static Guid? ResolveTemplate(string template)
        {
            Guid templateId;
            if (Guid.TryParse(template, out templateId))
            {
                return templateId;
            }

            var c1Template = PageTemplateFacade.GetPageTemplates().OfType<MvcPageTemplateDescriptor>().FirstOrDefault(t => t.Title == template);

            return c1Template == null ? (Guid?)null : c1Template.Id;
        }
    }
}
