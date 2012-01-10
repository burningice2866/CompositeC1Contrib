using System.Web;

using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Core.WebClient;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    public class DownloadActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            string url = "InstalledPackages/CompositeC1Contrib.DownloadFoldersAsZip/generateZip.ashx?";

            var downloadMediaFolderActionToken = actionToken as DownloadMediaFolderActionToken;
            if (downloadMediaFolderActionToken != null)
            {
                var folder = downloadMediaFolderActionToken.MediaFileFolder;
                url += "mode=media&keypath=" + folder.KeyPath;
            }

            var downloadArchiveActionToken = actionToken as DownloadArchiveActionToken;
            if (downloadArchiveActionToken != null)
            {
                var archive = downloadArchiveActionToken.ArchiveId;
                url += "mode=media&archive=" + archive;
            }

            var downloadFileFolderActionToken = actionToken as DownloadFileFolderActionToken;
            if (downloadFileFolderActionToken != null)
            {
                url += "mode=file&folder=" + HttpUtility.UrlEncode(downloadFileFolderActionToken.Path);
            }

            var currentConsoleId = flowControllerServicesContainer.GetService<IManagementConsoleMessageService>().CurrentConsoleId;
            url = UrlUtils.ResolveAdminUrl(url);

            ConsoleMessageQueueFacade.Enqueue(new DownloadFileMessageQueueItem(url), currentConsoleId);

            return null;
        }
    }
}
