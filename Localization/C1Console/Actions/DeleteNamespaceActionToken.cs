using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;

namespace CompositeC1Contrib.Localization.C1Console.Actions
{
    [ActionExecutor(typeof(DeleteNamespaceActionExecutor))]
    public class DeleteNamespaceActionToken : ActionToken
    {
        private static readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Delete };

        public override IEnumerable<PermissionType> PermissionTypes => _permissionTypes;

        public override string Serialize()
        {
            return nameof(DeleteNamespaceActionToken);
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new DeleteNamespaceActionToken();
        }
    }

    public class DeleteNamespaceActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var namespaceToken = (NamespaceFolderEntityToken)entityToken;

            LocalizationsFacade.DeleteNamespace(namespaceToken.Namespace, namespaceToken.ResourceSet);

            var treeRefresher = new SpecificTreeRefresher(flowControllerServicesContainer);

            treeRefresher.PostRefreshMesseges(new LocalizationElementProviderEntityToken(namespaceToken.ResourceSet));

            return null;
        }
    }
}
