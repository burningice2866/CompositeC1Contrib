using System;
using System.Linq;
using System.Web.Mvc.Routing;
using System.Web.Routing;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1PageTypeRouteAttribute : RouteFactoryAttribute
    {
        public Guid PageTypeId { get; set; }

        public C1PageTypeRouteAttribute(string template, string type)
            : base(template)
        {
            Guid typeId;
            if (!Guid.TryParse(type, out typeId))
            {
                using (var data = new DataConnection())
                {
                    var pageType = data.Get<IPageType>().FirstOrDefault(p => p.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
                    if (pageType != null)
                    {
                        typeId = pageType.Id;
                    }
                }
            }

            PageTypeId = typeId;
        }

        public override RouteValueDictionary Constraints
        {
            get
            {
                var constraints = new RouteValueDictionary 
                {
                    {"type", new C1PageTypeRouteConstraint(PageTypeId)}
                };

                return constraints;
            }
        }
    }
}
