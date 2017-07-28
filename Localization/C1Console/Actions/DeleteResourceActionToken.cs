using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.C1Console.Users;
using Composite.Data;

namespace CompositeC1Contrib.Localization.C1Console.Actions
{
    [ActionExecutor(typeof(DeleteResourceActionExecutor))]
    public class DeleteResourceActionToken : ActionToken
    {
        private static readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Delete };

        public override IEnumerable<PermissionType> PermissionTypes => _permissionTypes;

        public override string Serialize()
        {
            return nameof(DeleteResourceActionToken);
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new DeleteResourceActionToken();
        }
    }

    public class DeleteResourceActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var key = (IResourceKey)((DataEntityToken)entityToken).Data;
            var resourceManager = new C1ResourceDataManager(key.ResourceSet, UserSettings.ActiveLocaleCultureInfo);

            resourceManager.DeleteResource(key.Key);

            new ParentTreeRefresher(flowControllerServicesContainer).PostRefreshMesseges(entityToken);

            return null;
        }
    }
}
