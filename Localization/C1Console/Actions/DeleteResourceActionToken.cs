using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;

namespace CompositeC1Contrib.Localization.C1Console.Actions
{
    [ActionExecutor(typeof(DeleteResourceActionExecutor))]
    public class DeleteResourceActionToken : ActionToken
    {
        static private readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Administrate };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return "DeleteResourceActionToken";
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
            var dataToken = (DataEntityToken)entityToken;
            var key = (IResourceKey)dataToken.Data;

            using (var data = new DataConnection())
            {
                var values = data.Get<IResourceValue>().Where(v => v.KeyId == key.Id);

                data.Delete<IResourceValue>(values);
                data.Delete(key);
            }

            new ParentTreeRefresher(flowControllerServicesContainer).PostRefreshMesseges(entityToken);

            return null;
        }
    }
}
