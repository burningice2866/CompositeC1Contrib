using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Workflow.Activities;

using Composite.Data;

using CompositeC1Contrib.FormBuilder.Data.Types;

namespace CompositeC1Contrib.FormBuilder.Workflows
{
    public sealed partial class CreateFormWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public CreateFormWorkflow()
        {
            InitializeComponent();
        }

        public static IEnumerable<string> GetNetworkDeliveryOptions()
        {
            return Enum.GetNames(typeof(SmtpDeliveryMethod));
        }

        private void validateSave(object sender, ConditionalEventArgs e)
        {
            var form = GetBinding<IForm>("Form");

            using (var data = new DataConnection())
            {
                var nameExists = data.Get<IForm>().Any(q => q.Name == form.Name);

                if (nameExists)
                {
                    ShowFieldMessage("Form.Label", "Form with this name already exists");

                    e.Result = false;

                    return;
                }
            }

            e.Result = true;
        }

        private void initCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            if (!BindingExist("Form"))
            {
                using (var data = new DataConnection())
                {
                    var form = data.CreateNew<IForm>();

                    form.Id = Guid.NewGuid();

                    Bindings.Add("Form", form);
                }
            }
        }

        private void saveCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var form = GetBinding<IForm>("Form");

            using (var data = new DataConnection())
            {
                form = data.Add(form);
            }

            var newQueueEntityToken = form.GetDataEntityToken();
            var addNewTreeRefresher = CreateAddNewTreeRefresher(EntityToken);

            addNewTreeRefresher.PostRefreshMesseges(newQueueEntityToken);

            ExecuteWorklow(newQueueEntityToken, typeof(EditFormWorkflow));
        }
    }
}
