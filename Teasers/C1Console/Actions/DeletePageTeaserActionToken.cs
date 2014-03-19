using System;
using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;

using CompositeC1Contrib.Teasers.C1Console.EntityTokens;

namespace CompositeC1Contrib.Teasers.C1Console.Actions
{
    [ActionExecutor(typeof(DeletePageTeaserActionExecutor))]
    public class DeletePageTeaserActionToken : ActionToken
    {
        private static readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Delete, PermissionType.Edit, PermissionType.Administrate };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return String.Empty;
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new DeletePageTeaserActionToken();
        }
    }

    public class DeletePageTeaserActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var teaserInstance = (PageTeaserInstanceEntityToken)entityToken;
            var data = EntityTokenSerializer.Deserialize<DataEntityToken>(teaserInstance.Id).Data;

            DataFacade.Delete(data);

            new ParentTreeRefresher(flowControllerServicesContainer).PostRefreshMesseges(entityToken);

            return null;
        }
    }
}
