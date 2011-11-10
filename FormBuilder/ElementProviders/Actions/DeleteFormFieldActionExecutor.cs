using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;

namespace CompositeC1Contrib.FormBuilder.ElementProviders.Actions
{
    public class DeleteFormFieldActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var dataToken = (DataEntityToken)entityToken;
            var formField = dataToken.Data;

            using (var data = new DataConnection())
            {
                data.Delete(formField);
            }

            var treeRefresher = new ParentTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(entityToken);

            return null;
        }
    }
}
