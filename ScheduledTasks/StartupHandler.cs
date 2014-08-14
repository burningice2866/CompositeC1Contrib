using Composite.C1Console.Workflow;
using Composite.Core.Application;

namespace CompositeC1Contrib.ScheduledTasks
{
    [ApplicationStartup]
    public class StartupHandler
    {
        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            var workflowInstance = WorkflowFacade.CreateNewWorkflow(typeof(ScheduledTasksWorkflow));
            workflowInstance.Start();

            WorkflowFacade.RunWorkflow(workflowInstance);
        }
    }
}
