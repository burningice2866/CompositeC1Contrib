using System.Web;

using CompositeC1Contrib.NewRelic;

using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(AppStart), "Start")]

namespace CompositeC1Contrib.NewRelic
{
    public static class AppStart
    {
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(NewRelicHttpModule));
        }
    }
}
