using System;
using System.Linq;
using System.Workflow.Activities;

using Composite.C1Console.Workflow;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;
using CompositeC1Contrib.Email.ElementProviders.Tokens;

namespace CompositeC1Contrib.Email.Workflows
{
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public sealed partial class EditEmailQueueWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public EditEmailQueueWorkflow()
        {
            InitializeComponent();
        }

        private void initCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            if (!BindingExist("EmailQueue"))
            {
                var queueName = EntityToken.Id;
                using (var data = new DataConnection())
                {
                    var queue = data.Get<IEmailQueue>().Single(q => q.Name == queueName);

                    Bindings.Add("EmailQueue", queue);
                }
            }
        }

        private void validateSave(object sender, ConditionalEventArgs e)
        {
            var mailQueue = this.GetBinding<IEmailQueue>("EmailQueue");

            using (var data = new DataConnection())
            {
                var savedQueue = data.Get<IEmailQueue>().Single(q => q.Id == mailQueue.Id);

                if (savedQueue.Name != mailQueue.Name)
                {
                    var nameExists = data.Get<IEmailQueue>().Any(q => q.Name == mailQueue.Name);
                    if (nameExists)
                    {
                        ShowFieldMessage("Package.Name", "Package with this name already exists");

                        e.Result = false;

                        return;
                    }
                }
            }

            e.Result = true;
        }

        private void saveCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var mailQueue = this.GetBinding<IEmailQueue>("EmailQueue");

            using (var data = new DataConnection())
            {
                data.Update(mailQueue);
            }

            var treeRefresher = CreateSpecificTreeRefresher();
            treeRefresher.PostRefreshMesseges(new EmailElementProviderEntityToken());

            this.SetSaveStatus(true);
        }
    }
}
