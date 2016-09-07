using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Core.WebClient;

namespace CompositeC1Contrib.Localization.C1Console.Actions
{
    [ActionExecutor(typeof(DownloadExportedResourcesActionExecutor))]
    public class DownloadExportedResourcesActionToken : ActionToken
    {
        static private readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Read };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public string[] Languages { get; private set; }
        public string[] ResourceSets { get; private set; }
        public string Namespace { get; private set; }

        public DownloadExportedResourcesActionToken(string[] languages, string[] resourceSets, string ns)
        {
            Languages = languages;
            ResourceSets = resourceSets;
            Namespace = ns;
        }

        public override string Serialize()
        {
            var nvc = new NameValueCollection();

            nvc.Add("languages", String.Join(",", Languages));
            nvc.Add("resourceSets", String.Join(",", ResourceSets));
            nvc.Add("namespace", Namespace);

            return String.Join("&", nvc.AllKeys.Select(k => k + "=" + String.Join(",", nvc[k])));
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new GenerateClassWithKeysActionToken(serializedData);
        }
    }

    public class DownloadExportedResourcesActionExecutor : IActionExecutor
    {
        private static string LocalPath = "InstalledPackages/CompositeC1Contrib.Localization/Export.cshtml";

        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (DownloadExportedResourcesActionToken)actionToken;
            var currentConsoleId = flowControllerServicesContainer.GetService<IManagementConsoleMessageService>().CurrentConsoleId;

            var url = UrlUtils.ResolveAdminUrl(LocalPath + "?languages=" + String.Join(",", token.Languages) + "&resourceSets=" + String.Join(",", token.ResourceSets) + "&ns=" + token.Namespace);

            var downloadQueueItem = new DownloadFileMessageQueueItem(url);

            ConsoleMessageQueueFacade.Enqueue(downloadQueueItem, currentConsoleId);

            return null;
        }
    }
}
