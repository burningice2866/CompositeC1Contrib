using System;
using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class C1PageRouteAttribute : RouteFactoryAttribute
    {
        public Guid PageId { get; set; }

        public C1PageRouteAttribute(string template, string pageId)
            : base(template)
        {
            PageId = Guid.Parse(pageId);
        }

        public override RouteValueDictionary Constraints
        {
            get
            {
                var constraints = new RouteValueDictionary 
                {
                    {"pageId", new C1PageRouteConstraint(PageId)}
                };

                return constraints;
            }
        }
    }
}
