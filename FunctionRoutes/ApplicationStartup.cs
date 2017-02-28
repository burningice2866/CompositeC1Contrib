using System.Web.Routing;

using Composite.Core.Application;
using Composite.Data.DynamicTypes;

using CompositeC1Contrib.Web;

namespace CompositeC1Contrib.FunctionRoutes
{
    [ApplicationStartup]
    public class ApplicationStartup
    {
        public static void OnBeforeInitialize()
        {
            RouteTable.Routes.AddGenericHandler<FunctionRouteHandler>("function/{function}");
        }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IFunctionRoute));
        }
    }
}
