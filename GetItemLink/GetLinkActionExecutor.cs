using System;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Core.Routing;
using Composite.Core.WebClient;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.GetItemLink
{
    public class GetLinkActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var id = String.Empty;
            var title = String.Empty;
            var internalUrl = String.Empty;
            var publicUrl = String.Empty;

            var currentConsoleId = flowControllerServicesContainer.GetService<IManagementConsoleMessageService>().CurrentConsoleId;

            var mediaToken = actionToken as GetMediaLinkActionToken;
            if (mediaToken != null)
            {
                var file = mediaToken.File;

                id = file.Id.ToString();
                title = file.FileName;
                internalUrl = MediaUrlHelper.GetUrl(file);
                publicUrl = MediaUrlHelper.GetUrl(file, false);
            }

            var pageToken = actionToken as GetPageLinkActionToken;
            if (pageToken != null)
            {
                var page = pageToken.Page;

                using (var data = new DataConnection(PublicationScope.Published))
                {
                    var publishedPage = data.Get<IPage>().SingleOrDefault(p => p.Id == page.Id);
                    if (publishedPage != null)
                    {
                        page = publishedPage;
                    }

                    id = page.Id.ToString();
                    title = page.Title;
                    internalUrl = PageUrls.BuildUrl(page, UrlKind.Internal);
                    publicUrl = PageUrls.BuildUrl(page);
                }
            }

            ConsoleMessageQueueFacade.Enqueue(new MessageBoxMessageQueueItem
            {
                Title = title,
                Message = "Id: " + id + "\n\rInternal: " + internalUrl + "\n\rPublic url: " + publicUrl
            }, currentConsoleId);

            return null;
        }
    }
}
