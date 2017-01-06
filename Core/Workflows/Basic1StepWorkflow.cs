using System;
using System.Workflow.Activities;

namespace CompositeC1Contrib.Workflows
{
    public abstract class Basic1StepWorkflow : BaseFormsWorkflow
    {
        public abstract void OnInitialize(object sender, EventArgs e);
        public abstract void OnFinish(object sender, EventArgs e);

        protected void OnValidate(object sender, ConditionalEventArgs e)
        {
            e.Result = Validate();
        }

        public virtual bool Validate()
        {
            return true;
        }
    }
}
