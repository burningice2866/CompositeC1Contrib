using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

namespace CompositeC1Contrib.Email.ElementProviders.Actions
{
    [ActionExecutor(typeof(DeleteEmailQueueActionExecutor))]
    public class DeleteEmailQueueActionToken : ActionToken
    {
        static private IEnumerable<PermissionType> _permissionTypes = new PermissionType[] { PermissionType.Administrate };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return "DeleteEmailQueueActionToken";
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new DeleteEmailQueueActionToken();
        }
    }
}
