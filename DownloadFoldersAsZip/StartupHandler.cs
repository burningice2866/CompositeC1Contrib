using System.Web.Routing;

using Composite.Core.Application;
using Composite.Core.WebClient;

using CompositeC1Contrib.DownloadFoldersAsZip.Web;
using CompositeC1Contrib.Web;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [ApplicationStartup]
    public class StartupHandler
    {
        public const string Url = "InstalledPackages/CompositeC1Contrib.DownloadFoldersAsZip/generateZip.ashx";

        public static void OnBeforeInitialize()
        {
            var url = UrlUtils.ResolveAdminUrl(Url);

            if (url.StartsWith("/"))
            {
                url = url.Remove(0, 1);
            }

            RouteTable.Routes.AddGenericHandler<GenerateZipHandler>(url);
        }

        public static void OnInitialized() { }
    }
}
