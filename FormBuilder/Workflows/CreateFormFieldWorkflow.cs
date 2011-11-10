using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Workflow.Activities;

using Composite.C1Console.Trees.Workflows;
using Composite.C1Console.Workflow;
using Composite.Core.Serialization;
using Composite.Core.Types;
using Composite.Data;

using CompositeC1Contrib.FormBuilder.Data.Types;
using CompositeC1Contrib.FormBuilder.ElementProviders;
using CompositeC1Contrib.FormBuilder.Data;
using CompositeC1Contrib.FormBuilder.ElementProviders.Tokens;

namespace CompositeC1Contrib.FormBuilder.Workflows
{
    public sealed partial class CreateFormFieldWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public CreateFormFieldWorkflow()
        {
            InitializeComponent();
        }

        public static IEnumerable<string> GetKnownTypes()
        {
            return FormBuilderElementProvider.GetFormFieldTypes().Select(t => t.FullName);
        }

        public static IEnumerable<string> GetFormFieldTypes()
        {
            return Enum.GetNames(typeof(FormFieldType));
        }

        private void validateSave(object sender, ConditionalEventArgs e)
        {
            var formId = GetBinding<Guid>("FormId");
            var label = GetBinding<string>("Label");
            
            if (String.IsNullOrWhiteSpace(label))
            {
                ShowFieldMessage("Label", "Label cannot be empty");

                e.Result = false;

                return;
            }
                        
            foreach (var type in FormBuilderElementProvider.GetFormFieldTypes())
            {
                var labelExists = DataFacade.GetData(type).Cast<IFormField>().Any(f => f.FormId == formId && f.Label == label);
                if (labelExists)
                {
                    ShowFieldMessage("Label", "Form field with this label already exists");

                    e.Result = false;

                    return;
                }
            }

            e.Result = true;
        }

        private void initCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            if (!BindingExist("FormId"))
            {
                var folderToken = (FormFolderEntityToken)EntityToken;

                Bindings.Add("FormId", Guid.Parse(folderToken.Id));
                Bindings.Add("FieldType", GetFormFieldTypes().First());
                Bindings.Add("Label", String.Empty);
                Bindings.Add("DefaultValue", String.Empty);
                Bindings.Add("HelpText", String.Empty);
                Bindings.Add("ValidationRule", String.Empty);
                Bindings.Add("Type", GetKnownTypes().First());
            }
        }

        private void saveCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            var formId = GetBinding<Guid>("FormId");
            var fieldType = TypeManager.GetType(GetBinding<string>("Type"));
            var label = GetBinding<string>("Label");
            var defaultValue = GetBinding<string>("DefaultValue");
            var helpText = GetBinding<string>("HelpText");
            var validationRule = GetBinding<string>("ValidationRule");

            var payload = new StringBuilder();

            StringConversionServices.SerializeKeyValuePair(payload, "_InterfaceType_", fieldType);
            StringConversionServices.SerializeKeyValuePair(payload, "_IconResourceName_", "folder");

            StringConversionServices.SerializeKeyValuePair(payload, "FormId", formId);
            StringConversionServices.SerializeKeyValuePair(payload, "Type", fieldType);
            StringConversionServices.SerializeKeyValuePair(payload, "Label", label);
            StringConversionServices.SerializeKeyValuePair(payload, "DefaultValue", defaultValue);
            StringConversionServices.SerializeKeyValuePair(payload, "HelpText", helpText);
            StringConversionServices.SerializeKeyValuePair(payload, "ValidationRule", validationRule);

            var genericAddWorkflow = typeof(GenericAddDataWorkflow);
            var workflowToken = new WorkflowActionToken(genericAddWorkflow)
            {
                Payload = payload.ToString()
            };

            ExecuteAction(EntityToken, workflowToken);
        }
    }
}
