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
            var url = StartupHandler.Url + "?";

            var downloadActionToken = (DownloadActionToken)actionToken;

            switch (downloadActionToken.Type)
            {
                case "MediaArchive": url += "mode=media&archive=" + downloadActionToken.Path; break;
                case "MediaFolder": url += "mode=media&keypath=" + downloadActionToken.Path; break;
                case "File": url += "mode=file&folder=" + downloadActionToken.Path; break;
            }

            var currentConsoleId = flowControllerServicesContainer.GetService<IManagementConsoleMessageService>().CurrentConsoleId;
            url = UrlUtils.ResolveAdminUrl(url);

            ConsoleMessageQueueFacade.Enqueue(new DownloadFileMessageQueueItem(url), currentConsoleId);

            return null;
        }
    }
}
