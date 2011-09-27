using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;
using CompositeC1Contrib.Email.ElementProviders.Tokens;

namespace CompositeC1Contrib.Email.ElementProviders.Actions
{
    public class DeleteEmailQueueActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var dataToken = (DataEntityToken)entityToken;
            var queue = (IEmailQueue)dataToken.Data;

            using (var data = new DataConnection())
            {
                var messages = data.Get<IEmailMessage>().Where(m => m.QueueId == queue.Id).AsEnumerable();
                data.Delete(messages);

                data.Delete(queue);
            }

            var treeRefresher = new SpecificTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(new EmailElementProviderEntityToken());

            return null;
        }
    }
}
