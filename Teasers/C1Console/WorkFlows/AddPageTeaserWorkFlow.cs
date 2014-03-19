using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Composite.C1Console.Workflow;
using Composite.Core.Serialization;
using Composite.Data;

using CompositeC1Contrib.Teasers.Data;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Teasers.C1Console.WorkFlows
{
    public class AddPageTeaserWorkFlow : Basic1StepDialogWorkflow
    {
        public AddPageTeaserWorkFlow() : base("\\InstalledPackages\\CompositeC1Contrib.Teasers\\AddPageTeaserWorkflow.xml") { }

        public static Dictionary<string, string> GetPageTeaserTypes()
        {
            return TeaserFacade.GetPageTeaserTypes().ToDictionary(t => t.AssemblyQualifiedName, GetTitle);
        }

        private static string GetTitle(Type t)
        {
            var titleAttribute = t.GetCustomAttributes(typeof(TitleAttribute), true).Cast<TitleAttribute>().SingleOrDefault();

            return titleAttribute != null ? titleAttribute.Title : t.Name;
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var name = GetBinding<string>("Name");

            var teaserType = Type.GetType(GetBinding<string>("PageTeaserType"));
            var editWorkflowAttribute = teaserType.GetCustomAttributes(typeof(EditWorkflowAttribute), true).Cast<EditWorkflowAttribute>().Single();

            var payload = new StringBuilder();
            StringConversionServices.SerializeKeyValuePair(payload, "teaserType", teaserType.AssemblyQualifiedName);
            StringConversionServices.SerializeKeyValuePair(payload, "name", name);

            var workflowToken = new WorkflowActionToken(editWorkflowAttribute.EditWorkflowType)
            {
                Payload = payload.ToString()
            };

            CreateAddNewTreeRefresher(EntityToken).PostRefreshMesseges(EntityToken);
            ExecuteAction(EntityToken, workflowToken);
        }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (!BindingExist("PageTeaserType"))
            {
                Bindings.Add("PageTeaserType", TeaserFacade.GetPageTeaserTypes().First().AssemblyQualifiedName);
                Bindings.Add("Name", String.Empty);
            }
        }
    }
}
