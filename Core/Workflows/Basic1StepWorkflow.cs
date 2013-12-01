using System;
using System.Workflow.Activities;
using Composite.C1Console.Workflow.Activities;

namespace CompositeC1Contrib.Workflows
{
    public abstract class Basic1StepWorkflow : FormsWorkflow
    {
        public abstract void OnInitialize(object sender, EventArgs e);
        public abstract void OnFinish(object sender, EventArgs e);
        public virtual bool Validate()
        {
            return true;
        }

        protected void OnValidate(object sender, ConditionalEventArgs e)
        {
            e.Result = Validate();
        }
    }
}
