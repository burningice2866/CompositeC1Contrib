using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Workflow.Activities;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.Workflows
{
    public sealed partial class CreateEmailQueueWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public CreateEmailQueueWorkflow()
        {
            InitializeComponent();
        }

        public static IEnumerable<string> GetNetworkDeliveryOptions()
        {
            return Enum.GetNames(typeof(SmtpDeliveryMethod));
        }

        private void validateSave(object sender, ConditionalEventArgs e)
        {
            var mailQueue = GetBinding<IEmailQueue>("EmailQueue");

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
            if (!BindingExist("EmailQueue"))
            {
                using (var data = new DataConnection())
                {
                    var mailQueue = data.CreateNew<IEmailQueue>();

                    mailQueue.Id = Guid.NewGuid();
                    mailQueue.Port = 25;
                    mailQueue.Host = "localhost";
                    mailQueue.DeliveryMethod = SmtpDeliveryMethod.Network.ToString();

                    Bindings.Add("EmailQueue", mailQueue);
                }
            }
        }

        private void saveCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var mailQueue = GetBinding<IEmailQueue>("EmailQueue");

            using (var data = new DataConnection())
            {
                mailQueue = data.Add(mailQueue);
            }

            var newQueueEntityToken = mailQueue.GetDataEntityToken();
            var addNewTreeRefresher = CreateAddNewTreeRefresher(EntityToken);

            addNewTreeRefresher.PostRefreshMesseges(newQueueEntityToken);

            ExecuteWorklow(newQueueEntityToken, typeof(EditEmailQueueWorkflow));
        }
    }
}
