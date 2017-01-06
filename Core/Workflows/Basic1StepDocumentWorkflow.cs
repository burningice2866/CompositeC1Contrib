using System;
using System.Workflow.Activities;

using Composite.C1Console.Workflow;
using Composite.C1Console.Workflow.Activities;

namespace CompositeC1Contrib.Workflows
{
    public abstract class Basic1StepDocumentWorkflow : Basic1StepWorkflow
    {
        protected Basic1StepDocumentWorkflow() : this(null, String.Empty) { }

        protected Basic1StepDocumentWorkflow(string formDefinitionFile) : this(formDefinitionFile, String.Empty) { }

        protected Basic1StepDocumentWorkflow(string formDefinitionFile, string containerLabel)
        {
            CanModifyActivities = true;

            var codecondition1 = new CodeCondition();
            var saveCodeActivity = new CodeActivity();
            var elseBranchActivity = new IfElseBranchActivity();
            var ifValidateActivity = new IfElseBranchActivity();
            var setStateActivity2 = new SetStateActivity();
            var ifElseActivity1 = new IfElseActivity();
            var saveHandleExternalEventActivity1 = new SaveHandleExternalEventActivity();

            var initCodeActivity = new CodeActivity();
            var setStateActivity1 = new SetStateActivity();
            var cancelHandleExternalEventActivity1 = new CancelHandleExternalEventActivity();
            var eventDrivenActivitySave = new EventDrivenActivity();
            var stateInitializationActivity = new StateInitializationActivity();
            var globalEventDrivenActivity = new EventDrivenActivity();
            var finalState = new StateActivity();
            var initializationState = new StateActivity();
            // 
            // SaveCodeActivity
            // 
            saveCodeActivity.Name = "SaveCodeActivity";
            saveCodeActivity.ExecuteCode += (sender, e) => { SetCultureInfo(); OnFinish(sender, e); };

            // 
            // elseBranchActivity
            // 
            elseBranchActivity.Name = "elseBranchActivity";
            // 
            // ifValidateActivity
            // 
            ifValidateActivity.Activities.Add(saveCodeActivity);
            codecondition1.Condition += (sender, e) => { SetCultureInfo(); OnValidate(sender, e); };
            ifValidateActivity.Condition = codecondition1;
            ifValidateActivity.Name = "ifValidateActivity";
            // 
            // setStateActivity2
            // 
            setStateActivity2.Name = "setStateActivity2";
            setStateActivity2.TargetStateName = "initializationState";
            // 
            // ifElseActivity1
            // 
            ifElseActivity1.Activities.Add(ifValidateActivity);
            ifElseActivity1.Activities.Add(elseBranchActivity);
            ifElseActivity1.Name = "ifElseActivity1";
            // 
            // saveHandleExternalEventActivity1
            // 
            saveHandleExternalEventActivity1.EventName = "Save";
            saveHandleExternalEventActivity1.InterfaceType = typeof(IFormsWorkflowEventService);
            saveHandleExternalEventActivity1.Name = "saveHandleExternalEventActivity1";

            if (formDefinitionFile != null)
            {
                var documentFormActivity1 = new DocumentFormActivity
                {
                    ContainerLabel = containerLabel,
                    CustomToolbarDefinitionFileName = String.Empty,
                    FormDefinitionFileName = formDefinitionFile,
                    Name = "documentFormActivity1"
                };

                stateInitializationActivity.Activities.Add(documentFormActivity1);
            }

            // 
            // initCodeActivity
            // 
            initCodeActivity.Name = "initCodeActivity";
            initCodeActivity.ExecuteCode += (sender, e) => { SetCultureInfo(); OnInitialize(sender, e); };
            // 
            // setStateActivity1
            // 
            setStateActivity1.Name = "setStateActivity1";
            setStateActivity1.TargetStateName = "finalState";
            // 
            // cancelHandleExternalEventActivity1
            // 
            cancelHandleExternalEventActivity1.EventName = "Cancel";
            cancelHandleExternalEventActivity1.InterfaceType = typeof(IFormsWorkflowEventService);
            cancelHandleExternalEventActivity1.Name = "cancelHandleExternalEventActivity1";
            // 
            // eventDrivenActivity_Save
            // 
            eventDrivenActivitySave.Activities.Add(saveHandleExternalEventActivity1);
            eventDrivenActivitySave.Activities.Add(ifElseActivity1);
            eventDrivenActivitySave.Activities.Add(setStateActivity2);
            eventDrivenActivitySave.Name = "eventDrivenActivity_Save";
            // 
            // stateInitializationActivity
            // 
            stateInitializationActivity.Activities.Add(initCodeActivity);
            stateInitializationActivity.Name = "stateInitializationActivity";
            // 
            // GlobalEventDrivenActivity
            // 
            globalEventDrivenActivity.Activities.Add(cancelHandleExternalEventActivity1);
            globalEventDrivenActivity.Activities.Add(setStateActivity1);
            globalEventDrivenActivity.Name = "GlobalEventDrivenActivity";
            // 
            // finalState
            // 
            finalState.Name = "finalState";
            // 
            // initializationState
            // 
            initializationState.Activities.Add(stateInitializationActivity);
            initializationState.Activities.Add(eventDrivenActivitySave);
            initializationState.Name = "initializationState";
            // 
            // EditFormWorkflow
            // 
            Activities.Add(initializationState);
            Activities.Add(finalState);
            Activities.Add(globalEventDrivenActivity);
            CompletedStateName = "finalState";
            DynamicUpdateCondition = null;
            InitialStateName = "initializationState";
            Name = "EditFormWorkflow";
            CanModifyActivities = false;
        }
    }
}
