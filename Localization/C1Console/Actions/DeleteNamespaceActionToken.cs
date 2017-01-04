using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;

namespace CompositeC1Contrib.Localization.C1Console.Actions
{
    [ActionExecutor(typeof(DeleteNamespaceActionExecutor))]
    public class DeleteNamespaceActionToken : ActionToken
    {
        static private readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Delete };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return "DeleteNamespaceActionExecutor";
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
            var ns = ((NamespaceFolderEntityToken)entityToken).Namespace;

            LocalizationsFacade.DeleteNamespace(ns);

            var treeRefresher = new SpecificTreeRefresher(flowControllerServicesContainer);

            treeRefresher.PostRefreshMesseges(new LocalizationElementProviderEntityToken());

            return null;
        }
    }
}
