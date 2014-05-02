using System;
using System.Linq;

using Composite.C1Console.Workflow;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Security.Data.Types;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Security.Workflows
{
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public sealed partial class EditPagePermissionsWorkflow : Basic1StepDialogWorkflow
    {
        public EditPagePermissionsWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.Security\\EditPagePermissionsWorkflow.xml") { }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("PagePermissions"))
            {
                return;
            }

            var dataToken = (DataEntityToken)EntityToken;
            var page = (IPage)dataToken.Data;

            using (var data = new DataConnection())
            {
                var pagePermissions = data.Get<IPagePermissions>().SingleOrDefault(p => p.PageId == page.Id) ?? data.CreateNew<IPagePermissions>();

                Bindings.Add("PagePermissions", pagePermissions);
            }

            Bindings.Add("Title", page.Title);
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var pagePermissions = GetBinding<IPagePermissions>("PagePermissions");

            using (var data = new DataConnection())
            {
                if (pagePermissions.PageId == Guid.Empty)
                {
                    var dataToken = (DataEntityToken)EntityToken;
                    var page = (IPage)dataToken.Data;

                    pagePermissions.PageId = page.Id;

                    data.Add(pagePermissions);
                }
                else
                {
                    data.Update(pagePermissions);
                }
            }

            var treeRefresher = CreateSpecificTreeRefresher();
            treeRefresher.PostRefreshMesseges(EntityToken);

            SetSaveStatus(true);
        }
    }
}
