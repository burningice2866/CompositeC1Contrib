using System;
using System.Linq;
using System.Workflow.Activities;

using Composite.C1Console.Workflow;
using Composite.Data;

using CompositeC1Contrib.FormBuilder.Data.Types;
using CompositeC1Contrib.FormBuilder.ElementProviders.Tokens;

namespace CompositeC1Contrib.FormBuilder.Workflows
{
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public sealed partial class EditFormWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public EditFormWorkflow()
        {
            InitializeComponent();
        }

        private void initCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            if (!BindingExist("Form"))
            {
                var dataToken = (DataEntityToken)EntityToken;
                var form = (IForm)dataToken.Data;

                Bindings.Add("Form", form);
            }
        }

        private void validateSave(object sender, ConditionalEventArgs e)
        {
            var form = this.GetBinding<IForm>("Form");

            using (var data = new DataConnection())
            {
                var savedForm = data.Get<IForm>().Single(q => q.Id == form.Id);

                if (savedForm.Name != form.Name)
                {
                    var nameExists = data.Get<IForm>().Any(q => q.Name == form.Name);
                    if (nameExists)
                    {
                        ShowFieldMessage("Form.Name", "Form with this name already exists");

                        e.Result = false;

                        return;
                    }
                }
            }

            e.Result = true;
        }

        private void saveCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var form = this.GetBinding<IForm>("Form");

            using (var data = new DataConnection())
            {
                data.Update(form);
            }

            var treeRefresher = CreateSpecificTreeRefresher();
            treeRefresher.PostRefreshMesseges(new FormElementProviderEntityToken());

            this.SetSaveStatus(true);
        }
    }
}
