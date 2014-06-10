using System;
using System.Workflow.Activities;

using Composite.C1Console.Workflow;
using Composite.C1Console.Workflow.Activities;

namespace CompositeC1Contrib.Workflows
{
    public abstract class Basic1StepDialogWorkflow : Basic1StepWorkflow
    {
        protected Basic1StepDialogWorkflow() : this(null, String.Empty) { }

        protected Basic1StepDialogWorkflow(string formDefinitionFile) : this(formDefinitionFile, String.Empty) { }

        protected Basic1StepDialogWorkflow(string formDefinitionFile, string containerLabel)
        {
            CanModifyActivities = true;

            var codecondition1 = new CodeCondition();
            var setStateActivity6 = new SetStateActivity();
            var setStateActivity5 = new SetStateActivity();
            var saveCodeActivity = new CodeActivity();
            var ifElseBranchActivity2 = new IfElseBranchActivity();
            var ifElseBranchActivity1 = new IfElseBranchActivity();
            var ifElseActivity = new IfElseActivity();
            var finishHandleExternalEventActivity = new FinishHandleExternalEventActivity();
            var setStateActivity4 = new SetStateActivity();
            var cancelHandleExternalEventActivity1 = new CancelHandleExternalEventActivity();

            var initCodeActivity = new CodeActivity();
            var setStateActivity2 = new SetStateActivity();
            var drivenActivityOk = new EventDrivenActivity();
            var drivenActivityCancel = new EventDrivenActivity();
            var initializationActivity = new StateInitializationActivity();
            var setStateActivity1 = new SetStateActivity();
            var cancelHandleExternalEventActivity2 = new CancelHandleExternalEventActivity();
            var stateInitializationActivity = new StateInitializationActivity();
            var startState = new StateActivity();
            var globalCancelEventDrivenActivity = new EventDrivenActivity();
            var finalState = new StateActivity();
            var initializationState = new StateActivity();
            // 
            // setStateActivity6
            // 
            setStateActivity6.Name = "setStateActivity6";
            setStateActivity6.TargetStateName = "startState";
            // 
            // setStateActivity5
            // 
            setStateActivity5.Name = "setStateActivity5";
            setStateActivity5.TargetStateName = "finalState";
            // 
            // saveCodeActivity
            // 
            saveCodeActivity.Name = "saveCodeActivity";
            saveCodeActivity.ExecuteCode += OnFinish;
            // 
            // ifElseBranchActivity2
            // 
            ifElseBranchActivity2.Activities.Add(setStateActivity6);
            ifElseBranchActivity2.Name = "ifElseBranchActivity2";
            // 
            // ifElseBranchActivity1
            // 
            ifElseBranchActivity1.Activities.Add(saveCodeActivity);
            ifElseBranchActivity1.Activities.Add(setStateActivity5);
            codecondition1.Condition += OnValidate;
            ifElseBranchActivity1.Condition = codecondition1;
            ifElseBranchActivity1.Name = "ifElseBranchActivity1";
            // 
            // ifElseActivity
            // 
            ifElseActivity.Activities.Add(ifElseBranchActivity1);
            ifElseActivity.Activities.Add(ifElseBranchActivity2);
            ifElseActivity.Name = "ifElseActivity";
            // 
            // finishHandleExternalEventActivity
            // 
            finishHandleExternalEventActivity.EventName = "Finish";
            finishHandleExternalEventActivity.InterfaceType = typeof(IFormsWorkflowEventService);
            finishHandleExternalEventActivity.Name = "finishHandleExternalEventActivity";
            // 
            // setStateActivity4
            // 
            setStateActivity4.Name = "setStateActivity4";
            setStateActivity4.TargetStateName = "finalState";
            // 
            // cancelHandleExternalEventActivity1
            // 
            cancelHandleExternalEventActivity1.EventName = "Cancel";
            cancelHandleExternalEventActivity1.InterfaceType = typeof(IFormsWorkflowEventService);
            cancelHandleExternalEventActivity1.Name = "cancelHandleExternalEventActivity1";

            if (formDefinitionFile != null)
            {
                var dataDialogFormActivity = new DataDialogFormActivity
                {
                    ContainerLabel = containerLabel,
                    FormDefinitionFileName = formDefinitionFile,
                    Name = "dataDialogFormActivity"
                };

                initializationActivity.Activities.Add(dataDialogFormActivity);
            }

            // 
            // initCodeActivity
            // 
            initCodeActivity.Name = "initCodeActivity";
            initCodeActivity.ExecuteCode += OnInitialize;
            // 
            // setStateActivity2
            // 
            setStateActivity2.Name = "setStateActivity2";
            setStateActivity2.TargetStateName = "startState";
            // 
            // DrivenActivity_Ok
            // 
            drivenActivityOk.Activities.Add(finishHandleExternalEventActivity);
            drivenActivityOk.Activities.Add(ifElseActivity);
            drivenActivityOk.Name = "DrivenActivity_Ok";
            // 
            // DrivenActivity_Cancel
            // 
            drivenActivityCancel.Activities.Add(cancelHandleExternalEventActivity1);
            drivenActivityCancel.Activities.Add(setStateActivity4);
            drivenActivityCancel.Name = "DrivenActivity_Cancel";
            // 
            // initializationActivity
            // 
            initializationActivity.Activities.Add(initCodeActivity);

            initializationActivity.Name = "initializationActivity";
            // 
            // setStateActivity1
            // 
            setStateActivity1.Name = "setStateActivity1";
            setStateActivity1.TargetStateName = "finalState";
            // 
            // cancelHandleExternalEventActivity2
            // 
            cancelHandleExternalEventActivity2.EventName = "Cancel";
            cancelHandleExternalEventActivity2.InterfaceType = typeof(IFormsWorkflowEventService);
            cancelHandleExternalEventActivity2.Name = "cancelHandleExternalEventActivity2";
            // 
            // stateInitializationActivity
            // 
            stateInitializationActivity.Activities.Add(setStateActivity2);
            stateInitializationActivity.Name = "stateInitializationActivity";
            // 
            // startState
            // 
            startState.Activities.Add(initializationActivity);
            startState.Activities.Add(drivenActivityCancel);
            startState.Activities.Add(drivenActivityOk);
            startState.Name = "startState";
            // 
            // globalCancelEventDrivenActivity
            // 
            globalCancelEventDrivenActivity.Activities.Add(cancelHandleExternalEventActivity2);
            globalCancelEventDrivenActivity.Activities.Add(setStateActivity1);
            globalCancelEventDrivenActivity.Name = "globalCancelEventDrivenActivity";
            // 
            // finalState
            // 
            finalState.Name = "finalState";
            // 
            // initializationState
            // 
            initializationState.Activities.Add(stateInitializationActivity);
            initializationState.Name = "initializationState";
            // 
            // AddFormWorkflow
            // 
            Activities.Add(initializationState);
            Activities.Add(finalState);
            Activities.Add(globalCancelEventDrivenActivity);
            Activities.Add(startState);
            CompletedStateName = "finalState";
            DynamicUpdateCondition = null;
            InitialStateName = "initializationState";
            Name = "AddFormWorkflow";
            CanModifyActivities = false;
        }
    }
}