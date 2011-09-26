using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace CompositeC1Contrib.Email.Workflows
{
    partial class CreateEmailQueueWorkflow
    {
        #region Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        private void InitializeComponent()
        {
            this.CanModifyActivities = true;
            System.Workflow.Activities.CodeCondition codecondition1 = new System.Workflow.Activities.CodeCondition();
            this.faultHandlersActivity1 = new System.Workflow.ComponentModel.FaultHandlersActivity();
            this.setStateActivity6 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity5 = new System.Workflow.Activities.SetStateActivity();
            this.saveCodeActivity = new System.Workflow.Activities.CodeActivity();
            this.ifElseBranchActivity2 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity1 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseActivity1 = new System.Workflow.Activities.IfElseActivity();
            this.nextHandleExternalEventActivity2 = new Composite.C1Console.Workflow.Activities.NextHandleExternalEventActivity();
            this.setStateActivity4 = new System.Workflow.Activities.SetStateActivity();
            this.cancelHandleExternalEventActivity1 = new Composite.C1Console.Workflow.Activities.CancelHandleExternalEventActivity();
            this.wizardFormActivity1 = new Composite.C1Console.Workflow.Activities.WizardFormActivity();
            this.initCodeActivity = new System.Workflow.Activities.CodeActivity();
            this.setStateActivity2 = new System.Workflow.Activities.SetStateActivity();
            this.NextDrivenActivity1 = new System.Workflow.Activities.EventDrivenActivity();
            this.CancelDrivenActivity2 = new System.Workflow.Activities.EventDrivenActivity();
            this.initializationActivity = new System.Workflow.Activities.StateInitializationActivity();
            this.setStateActivity1 = new System.Workflow.Activities.SetStateActivity();
            this.cancelHandleExternalEventActivity2 = new Composite.C1Console.Workflow.Activities.CancelHandleExternalEventActivity();
            this.stateInitializationActivity = new System.Workflow.Activities.StateInitializationActivity();
            this.startState = new System.Workflow.Activities.StateActivity();
            this.globalCancelEventDrivenActivity = new System.Workflow.Activities.EventDrivenActivity();
            this.finalState = new System.Workflow.Activities.StateActivity();
            this.initializationState = new System.Workflow.Activities.StateActivity();
            // 
            // faultHandlersActivity1
            // 
            this.faultHandlersActivity1.Name = "faultHandlersActivity1";
            // 
            // setStateActivity6
            // 
            this.setStateActivity6.Name = "setStateActivity6";
            this.setStateActivity6.TargetStateName = "startState";
            // 
            // setStateActivity5
            // 
            this.setStateActivity5.Name = "setStateActivity5";
            this.setStateActivity5.TargetStateName = "finalState";
            // 
            // saveCodeActivity
            // 
            this.saveCodeActivity.Name = "saveCodeActivity";
            this.saveCodeActivity.ExecuteCode += new System.EventHandler(this.saveCodeActivity_ExecuteCode);
            // 
            // ifElseBranchActivity2
            // 
            this.ifElseBranchActivity2.Activities.Add(this.setStateActivity6);
            this.ifElseBranchActivity2.Activities.Add(this.faultHandlersActivity1);
            this.ifElseBranchActivity2.Name = "ifElseBranchActivity2";
            // 
            // ifElseBranchActivity1
            // 
            this.ifElseBranchActivity1.Activities.Add(this.saveCodeActivity);
            this.ifElseBranchActivity1.Activities.Add(this.setStateActivity5);
            codecondition1.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.validateSave);
            this.ifElseBranchActivity1.Condition = codecondition1;
            this.ifElseBranchActivity1.Name = "ifElseBranchActivity1";
            // 
            // ifElseActivity1
            // 
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity1);
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity2);
            this.ifElseActivity1.Name = "ifElseActivity1";
            // 
            // nextHandleExternalEventActivity2
            // 
            this.nextHandleExternalEventActivity2.EventName = "Next";
            this.nextHandleExternalEventActivity2.InterfaceType = typeof(Composite.C1Console.Workflow.IFormsWorkflowEventService);
            this.nextHandleExternalEventActivity2.Name = "nextHandleExternalEventActivity2";
            // 
            // setStateActivity4
            // 
            this.setStateActivity4.Name = "setStateActivity4";
            this.setStateActivity4.TargetStateName = "finalState";
            // 
            // cancelHandleExternalEventActivity1
            // 
            this.cancelHandleExternalEventActivity1.EventName = "Cancel";
            this.cancelHandleExternalEventActivity1.InterfaceType = typeof(Composite.C1Console.Workflow.IFormsWorkflowEventService);
            this.cancelHandleExternalEventActivity1.Name = "cancelHandleExternalEventActivity1";
            // 
            // wizardFormActivity1
            // 
            this.wizardFormActivity1.ContainerLabel = null;
            this.wizardFormActivity1.FormDefinitionFileName = "\\InstalledPackages\\CompositeC1Contrib.Email\\CreateEmailQueue.xml";
            this.wizardFormActivity1.Name = "wizardFormActivity1";
            // 
            // initCodeActivity
            // 
            this.initCodeActivity.Name = "initCodeActivity";
            this.initCodeActivity.ExecuteCode += new System.EventHandler(this.initCodeActivity_ExecuteCode);
            // 
            // setStateActivity2
            // 
            this.setStateActivity2.Name = "setStateActivity2";
            this.setStateActivity2.TargetStateName = "startState";
            // 
            // NextDrivenActivity1
            // 
            this.NextDrivenActivity1.Activities.Add(this.nextHandleExternalEventActivity2);
            this.NextDrivenActivity1.Activities.Add(this.ifElseActivity1);
            this.NextDrivenActivity1.Name = "NextDrivenActivity1";
            // 
            // CancelDrivenActivity2
            // 
            this.CancelDrivenActivity2.Activities.Add(this.cancelHandleExternalEventActivity1);
            this.CancelDrivenActivity2.Activities.Add(this.setStateActivity4);
            this.CancelDrivenActivity2.Name = "CancelDrivenActivity2";
            // 
            // initializationActivity
            // 
            this.initializationActivity.Activities.Add(this.initCodeActivity);
            this.initializationActivity.Activities.Add(this.wizardFormActivity1);
            this.initializationActivity.Name = "initializationActivity";
            // 
            // setStateActivity1
            // 
            this.setStateActivity1.Name = "setStateActivity1";
            this.setStateActivity1.TargetStateName = "finalState";
            // 
            // cancelHandleExternalEventActivity2
            // 
            this.cancelHandleExternalEventActivity2.EventName = "Cancel";
            this.cancelHandleExternalEventActivity2.InterfaceType = typeof(Composite.C1Console.Workflow.IFormsWorkflowEventService);
            this.cancelHandleExternalEventActivity2.Name = "cancelHandleExternalEventActivity2";
            // 
            // stateInitializationActivity
            // 
            this.stateInitializationActivity.Activities.Add(this.setStateActivity2);
            this.stateInitializationActivity.Name = "stateInitializationActivity";
            // 
            // startState
            // 
            this.startState.Activities.Add(this.initializationActivity);
            this.startState.Activities.Add(this.CancelDrivenActivity2);
            this.startState.Activities.Add(this.NextDrivenActivity1);
            this.startState.Name = "startState";
            // 
            // globalCancelEventDrivenActivity
            // 
            this.globalCancelEventDrivenActivity.Activities.Add(this.cancelHandleExternalEventActivity2);
            this.globalCancelEventDrivenActivity.Activities.Add(this.setStateActivity1);
            this.globalCancelEventDrivenActivity.Name = "globalCancelEventDrivenActivity";
            // 
            // finalState
            // 
            this.finalState.Name = "finalState";
            // 
            // initializationState
            // 
            this.initializationState.Activities.Add(this.stateInitializationActivity);
            this.initializationState.Name = "initializationState";
            // 
            // CreateEmailQueueWorkflow
            // 
            this.Activities.Add(this.initializationState);
            this.Activities.Add(this.finalState);
            this.Activities.Add(this.globalCancelEventDrivenActivity);
            this.Activities.Add(this.startState);
            this.CompletedStateName = "finalState";
            this.DynamicUpdateCondition = null;
            this.InitialStateName = "initializationState";
            this.Name = "CreateEmailQueueWorkflow";
            this.CanModifyActivities = false;

        }

        #endregion


        private StateInitializationActivity packageInfoInitialization;

        private StateActivity mailQueueState;

        private CodeActivity saveMailQueueCodeActivity;

        private CancellationHandlerActivity cancellationHandlerActivity1;
        private EventDrivenActivity globalCancelEventDrivenActivity;
        private StateActivity finalState;
        private Composite.C1Console.Workflow.Activities.WizardFormActivity wizardFormActivity1;
        private CodeActivity initCodeActivity;
        private SetStateActivity setStateActivity2;
        private SetStateActivity setStateActivity1;
        private StateInitializationActivity stateInitializationActivity;
        private SetStateActivity setStateActivity4;
        private Composite.C1Console.Workflow.Activities.CancelHandleExternalEventActivity cancelHandleExternalEventActivity1;
        private Composite.C1Console.Workflow.Activities.NextHandleExternalEventActivity nextHandleExternalEventActivity2;
        private EventDrivenActivity CancelDrivenActivity2;
        private EventDrivenActivity NextDrivenActivity1;
        private Composite.C1Console.Workflow.Activities.CancelHandleExternalEventActivity cancelHandleExternalEventActivity2;
        private FaultHandlersActivity faultHandlersActivity1;
        private SetStateActivity setStateActivity6;
        private SetStateActivity setStateActivity5;
        private IfElseBranchActivity ifElseBranchActivity2;
        private IfElseBranchActivity ifElseBranchActivity1;
        private IfElseActivity ifElseActivity1;
        private CodeActivity saveCodeActivity;
        private StateInitializationActivity initializationActivity;
        private StateActivity startState;
        private StateActivity initializationState;





































































    }
}
