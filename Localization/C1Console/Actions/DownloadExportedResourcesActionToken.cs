using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Core.WebClient;

namespace CompositeC1Contrib.Localization.C1Console.Actions
{
    [ActionExecutor(typeof(DownloadExportedResourcesActionExecutor))]
    public class DownloadExportedResourcesActionToken : ActionToken
    {
        private static readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Read };

        public override IEnumerable<PermissionType> PermissionTypes => _permissionTypes;

        public string[] Languages { get; }
        public string[] ResourceSets { get; }
        public string Namespace { get; }

        public DownloadExportedResourcesActionToken(string[] languages, string[] resourceSets, string ns)
        {
            Languages = languages;
            ResourceSets = resourceSets;
            Namespace = ns;
        }

        public override string Serialize()
        {
            var nvc = new NameValueCollection
            {
                {"languages", String.Join(",", Languages)},
                {"resourceSets", String.Join(",", ResourceSets)},
                {"namespace", Namespace}
            };

            return String.Join("&", nvc.AllKeys.Select(k => k + "=" + String.Join(",", nvc[k])));
        }

        public static ActionToken Deserialize(string serializedData)
        {
            var nvc = HttpUtility.ParseQueryString(serializedData);

            var languages = nvc["languages"].Split(',');
            var resourceSets = nvc["resourceSets"].Split(',');
            var ns = nvc["namespace"];

            return new DownloadExportedResourcesActionToken(languages, resourceSets, ns);
        }
    }

    public class DownloadExportedResourcesActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (DownloadExportedResourcesActionToken)actionToken;
            var currentConsoleId = flowControllerServicesContainer.GetService<IManagementConsoleMessageService>().CurrentConsoleId;

            var url = UrlUtils.ResolveAdminUrl(StartupHandler.Url + "?languages=" + String.Join(",", token.Languages) + "&resourceSets=" + String.Join(",", token.ResourceSets) + "&ns=" + token.Namespace);

            var downloadQueueItem = new DownloadFileMessageQueueItem(url);

            ConsoleMessageQueueFacade.Enqueue(downloadQueueItem, currentConsoleId);

            return null;
        }
    }
}
