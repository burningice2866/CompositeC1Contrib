using System;

namespace CompositeC1Contrib.Teasers.C1Console.WorkFlows
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EditWorkflowAttribute : Attribute
    {
        public Type EditWorkflowType { get; set; }

        public EditWorkflowAttribute(Type type)
        {
            EditWorkflowType = type;
        }
    }
}
