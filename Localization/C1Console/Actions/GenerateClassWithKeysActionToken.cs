using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Core.IO;
using Composite.Core.WebClient;

namespace CompositeC1Contrib.Localization.C1Console.Actions
{
    [ActionExecutor(typeof(GenerateClassWithKeysActionExecutor))]
    public class GenerateClassWithKeysActionToken : ActionToken
    {
        private static readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Read };

        public override IEnumerable<PermissionType> PermissionTypes => _permissionTypes;

        public string Namespace { get; }

        public GenerateClassWithKeysActionToken(string ns)
        {
            Namespace = ns;
        }

        public override string Serialize()
        {
            return Namespace;
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new GenerateClassWithKeysActionToken(serializedData);
        }
    }

    public class GenerateClassWithKeysActionExecutor : IActionExecutor
    {
        private const string LocalPath = "InstalledPackages/CompositeC1Contrib.Localization";

        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var ns = ((GenerateClassWithKeysActionToken)actionToken).Namespace;

            var resourceKeys = LocalizationsFacade.GetResourceKeys(String.Empty).Select(k => k.Key);
            var generator = new ClassKeysGenerator(resourceKeys, ns);

            var content = generator.Generate();

            var dir = PathUtil.Resolve("~" + Path.Combine("/", UrlUtils.AdminRootPath, LocalPath));
            if (!C1Directory.Exists(dir))
            {
                C1Directory.CreateDirectory(dir);
            }

            var file = Path.Combine(dir, "Resources.txt");
            C1File.WriteAllText(file, content);

            var url = UrlUtils.ResolveAdminUrl(UrlUtils.Combine(LocalPath, Path.GetFileName(file)));

            var currentConsoleId = flowControllerServicesContainer.GetService<IManagementConsoleMessageService>().CurrentConsoleId;

            var openViewMessageItem = new OpenViewMessageQueueItem
            {
                Label = "Resources.cs",
                Url = url,
                ViewId = Guid.NewGuid().ToString(),
                ViewType = ViewType.Main
            };

            ConsoleMessageQueueFacade.Enqueue(openViewMessageItem, currentConsoleId);

            return null;
        }
    }
}
