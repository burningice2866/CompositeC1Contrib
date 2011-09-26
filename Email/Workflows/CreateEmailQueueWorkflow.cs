using System;
using System.Linq;
using System.Workflow.Activities;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;
using CompositeC1Contrib.Email.ElementProviders.Tokens;

namespace CompositeC1Contrib.Email.Workflows
{
    public sealed partial class CreateEmailQueueWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public CreateEmailQueueWorkflow()
        {
            InitializeComponent();
        }

        private void validateSave(object sender, ConditionalEventArgs e)
        {
            var mailQueue = this.GetBinding<IEmailQueue>("EmailQueue");

            using (var data = new DataConnection())
            {
                var nameExists = data.Get<IEmailQueue>().Any(q => q.Name == mailQueue.Name);

                if (nameExists)
                {
                    ShowFieldMessage("Package.Name", "Package with this name already exists");

                    e.Result = false;

                    return;
                }
            }

            e.Result = true;
        }

        private void initCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            using (var data = new DataConnection())
            {
                var mailQueue = data.CreateNew<IEmailQueue>();

                mailQueue.Id = Guid.NewGuid();
                mailQueue.Name = "Enter name...";

                Bindings.Add("EmailQueue", mailQueue);
            }
        }

        private void saveCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var mailQueue = this.GetBinding<IEmailQueue>("EmailQueue");

            using (var data = new DataConnection())
            {
                data.Add(mailQueue);
            }

            var newQueueEntityToken = new EmailQueueEntityToken(mailQueue.Name);
            var addNewTreeRefresher = CreateAddNewTreeRefresher(EntityToken);

            addNewTreeRefresher.PostRefreshMesseges(newQueueEntityToken);

            ExecuteWorklow(newQueueEntityToken, typeof(EditEmailQueueWorkflow));
        }
    }
}
