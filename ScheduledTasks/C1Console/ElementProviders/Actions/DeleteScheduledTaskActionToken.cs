using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

using CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens;

using Hangfire;
using Hangfire.States;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.Actions
{
    [ActionExecutor(typeof(DeleteScheduledTaskActionExecutor))]
    public class DeleteScheduledTaskActionToken : ActionToken
    {
        static private readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Administrate };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return "DeleteScheduledTaskActionToken";
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new DeleteScheduledTaskActionToken();
        }
    }

    public class DeleteScheduledTaskActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (TaskEntityToken)entityToken;
            var client = new BackgroundJobClient(JobStorage.Current);

            client.Delete(token.Id, ScheduledState.StateName);

            var treeRefresher = new SpecificTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(new FolderEntityToken(TaskType.Scheduled));

            return null;
        }
    }
}
