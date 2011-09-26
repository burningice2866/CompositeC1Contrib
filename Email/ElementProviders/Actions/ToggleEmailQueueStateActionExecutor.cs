using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.ElementProviders
{
    public class ToggleEmailQueueStateActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var queueId = ((ToggleEmailQueueStateActionToken)actionToken).QueueId;
            
            using (var data = new DataConnection(PublicationScope.Unpublished))
            {
                var queue = data.Get<IEmailQueue>().Single(q => q.Id == queueId);

                queue.Paused = !queue.Paused;

                data.Update(queue);
            }

            EntityTokenCacheFacade.ClearCache();

            var treeRefresher = new ParentTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(entityToken);

            return null;
        }
    }
}
