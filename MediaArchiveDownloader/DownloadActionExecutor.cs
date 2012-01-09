using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Core.WebClient;

namespace CompositeC1Contrib.MediaArchiveDownloader
{
    public class DownloadActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            string url = "downloadmediafolder.ashx";

            var downloadFolderActionToken = actionToken as DownloadFolderActionToken;
            if (downloadFolderActionToken != null)
            {
                var folder = downloadFolderActionToken.MediaFileFolder;
                url += "?keypath=" + folder.KeyPath;
            }

            var downloadArchiveActionToken = actionToken as DownloadArchiveActionToken;
            if (downloadArchiveActionToken != null)
            {
                var archive = downloadArchiveActionToken.ArchiveId;
                url += "?archive=" + archive;
            }

            var currentConsoleId = flowControllerServicesContainer.GetService<IManagementConsoleMessageService>().CurrentConsoleId;
            url = UrlUtils.ResolveAdminUrl(url);

            ConsoleMessageQueueFacade.Enqueue(new DownloadFileMessageQueueItem(url), currentConsoleId);

            return null;
        }
    }
}
