using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

using CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens;

using Hangfire;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.Actions
{
    [ActionExecutor(typeof(TriggerRecurringTaskActionExecutor))]
    public class TriggerRecurringTaskActionToken : ActionToken
    {
        static private readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Administrate };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return "TriggerRecurringTaskActionToken";
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new TriggerRecurringTaskActionToken();
        }
    }

    public class TriggerRecurringTaskActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (TaskEntityToken)entityToken;
            var manager = new RecurringJobManager(JobStorage.Current);

            manager.Trigger(token.Id);

            var treeRefresher = new SpecificTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(new FolderEntityToken(TaskType.Recurring));

            return null;
        }
    }
}
