using System;
using System.Workflow.Runtime;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.Serialization;

namespace CompositeC1Contrib.Favorites.Workflows
{
    public sealed partial class ConfirmWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public ConfirmWorkflow()
        {
            InitializeComponent();
        }

        private void codeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var type = StringConversionServices.DeserializeValueType(StringConversionServices.ParseKeyValueCollection(Payload)["ActionToken"]);

            var actionToken = (ActionToken)Activator.CreateInstance(type);
            var container = WorkflowFacade.GetFlowControllerServicesContainer(WorkflowEnvironment.WorkflowInstanceId);

            ActionExecutorFacade.Execute(EntityToken, actionToken, container);
        }

        private void initCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var payLoad = StringConversionServices.ParseKeyValueCollection(Payload);
            var confirmMessage = StringConversionServices.DeserializeValueString(payLoad["ConfirmMessage"]);

            Bindings.Add("ConfirmMessage", confirmMessage);
        }
    }
}
