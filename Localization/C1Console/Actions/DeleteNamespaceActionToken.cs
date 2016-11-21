using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Transactions;

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

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    var keys = data.Get<IResourceKey>().Where(r => r.ResourceSet == null && r.Key.StartsWith(ns)).ToList();
                    foreach (var key in keys)
                    {
                        var values = data.Get<IResourceValue>().Where(v => v.KeyId == key.Id);

                        data.Delete<IResourceValue>(values);
                    }

                    data.Delete<IResourceKey>(keys);
                }
            }

            var treeRefresher = new SpecificTreeRefresher(flowControllerServicesContainer);

            treeRefresher.PostRefreshMesseges(new LocalizationElementProviderEntityToken());

            return null;
        }
    }
}
