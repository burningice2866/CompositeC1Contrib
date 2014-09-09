using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

using CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens;

using Hangfire;
using Hangfire.States;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.Actions
{
    [ActionExecutor(typeof(RequeueScheduledTaskActionExecutor))]
    public class RequeueScheduledTaskActionToken : ActionToken
    {
        static private readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Administrate };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return "RequeueScheduledTaskActionToken";
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new RequeueScheduledTaskActionToken();
        }
    }

    public class RequeueScheduledTaskActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (TaskEntityToken)entityToken;
            var client = new BackgroundJobClient(JobStorage.Current);

            client.Requeue(token.Id, ScheduledState.StateName);

            var treeRefresher = new SpecificTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(new FolderEntityToken(TaskType.Scheduled));

            return null;
        }
    }
}
