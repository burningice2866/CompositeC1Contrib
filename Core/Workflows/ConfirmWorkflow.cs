using System;
using System.IO;
using System.Reflection;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow.Activities;
using Composite.Core.Serialization;

namespace CompositeC1Contrib.Workflows
{
    public sealed partial class ConfirmWorkflow : FormsWorkflow
    {
        public ConfirmWorkflow()
        {
            InitializeComponent();            
        }

        private void codeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var type = StringConversionServices.DeserializeValueType(StringConversionServices.ParseKeyValueCollection(Payload)["Type"]);

            if (typeof(ActionToken).IsAssignableFrom(type))
            {
                var actionToken = (ActionToken)Activator.CreateInstance(type);

                ExecuteAction(EntityToken, actionToken);
            }

            if (typeof(FormsWorkflow).IsAssignableFrom(type))
            {
                ExecuteWorklow(EntityToken, type);
            }
        }

        private void initCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var payLoad = StringConversionServices.ParseKeyValueCollection(Payload);
            var confirmMessage = StringConversionServices.DeserializeValueString(payLoad["ConfirmMessage"]);

            Bindings.Add("ConfirmMessage", confirmMessage);

            var assembly = Assembly.GetAssembly(typeof(ConfirmWorkflow));
            using (var streamReader = new StreamReader(assembly.GetManifestResourceStream("CompositeC1Contrib.Workflows.ConfirmMessage.xml")))
            {
                var formData = streamReader.ReadToEnd();

                DeliverFormData("Confirm", StandardUiContainerTypes.ConfirmDialog, formData, Bindings, BindingsValidationRules);                
            }            
        }
    }
}
