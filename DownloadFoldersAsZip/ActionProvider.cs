using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Data.Types;
using Composite.Plugins.Elements.ElementProviders.MediaFileProviderElementProvider;
using Composite.Plugins.Elements.ElementProviders.WebsiteFileElementProvider;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class ActionProvider : IElementActionProvider
    {
        private static readonly ActionGroup _actionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation _actionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = false, ActionGroup = _actionGroup };

        public IEnumerable<ElementAction> GetActions(EntityToken entityToken)
        {
            var mediaRootEntityToken = entityToken as MediaRootFolderProviderEntityToken;
            if (mediaRootEntityToken != null)
            {
                var actionToken = new DownloadArchiveActionToken(mediaRootEntityToken.Id);
                yield return createElementAction(actionToken);
            }

            var dataEntityToken = entityToken as DataEntityToken;
            if (dataEntityToken != null)
            {
                var folder = dataEntityToken.Data as IMediaFileFolder;
                if (folder != null)
                {
                    var actionToken = new DownloadMediaFolderActionToken(folder);
                    yield return createElementAction(actionToken);
                }
            }            

            var websiteFileRootEntityToken = entityToken as WebsiteFileElementProviderRootEntityToken;
            if (websiteFileRootEntityToken != null)
            {
                var actionToken = new DownloadFileFolderActionToken(HostingEnvironment.MapPath("~"));
                yield return createElementAction(actionToken);
            }

            var websiteFileEntityToken = entityToken as WebsiteFileElementProviderEntityToken;
            if (websiteFileEntityToken != null)
            {
                var attr = File.GetAttributes(websiteFileEntityToken.Path);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    var actionToken = new DownloadFileFolderActionToken(websiteFileEntityToken.Path);
                    yield return createElementAction(actionToken);
                }
            }
        }

        private ElementAction createElementAction(ActionToken actionToken)
        {
            return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Download",
                    ToolTip = "Download",
                    Icon = new ResourceHandle("Composite.Icons", "down"),
                    ActionLocation = _actionLocation
                }
            };
        }
    }
}