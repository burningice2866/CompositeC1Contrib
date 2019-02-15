using Composite.C1Console.Elements;
using Composite.C1Console.Security;

using CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens;

namespace CompositeC1Contrib.ScheduledTasks.C1Console
{
    public class UrlToEntityTokenMapper : IUrlToEntityTokenMapper
    {
        public BrowserViewSettings TryGetBrowserViewSettings(EntityToken entityToken, bool showPublishedView)
        {
            return new BrowserViewSettings
            {
                Url = TryGetUrl(entityToken),
                ToolingOn = false
            };
        }

        public EntityToken TryGetEntityToken(string url)
        {
            return null;
        }

        public string TryGetUrl(EntityToken entityToken)
        {
            if (entityToken is ScheduledTasksElementProviderEntityToken)
            {
                return "/hangfire";
            }

            if (entityToken is FolderEntityToken folderToken)
            {
                switch (folderToken.TaskType)
                {
                    case TaskType.Scheduled: return "/hangfire/jobs/scheduled";
                    case TaskType.Recurring: return "/hangfire/recurring";
                    case TaskType.Enqueued: return "/hangfire/jobs/enqueued";
                }
            }

            return null;
        }
    }
}
