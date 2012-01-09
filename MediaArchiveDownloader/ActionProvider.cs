using System.Collections.Generic;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Data.Types;
using Composite.Plugins.Elements.ElementProviders.MediaFileProviderElementProvider;

namespace CompositeC1Contrib.MediaArchiveDownloader
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class ActionProvider : IElementActionProvider
    {
        private static readonly ActionGroup _actionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation _actionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = false, ActionGroup = _actionGroup };

        public IEnumerable<ElementAction> GetActions(EntityToken entityToken)
        {
            var dataEntityToken = entityToken as DataEntityToken;
            if (dataEntityToken != null)
            {
                var folder = dataEntityToken.Data as IMediaFileFolder;
                if (folder != null)
                {
                    var actionToken = new DownloadFolderActionToken(folder);

                    yield return new ElementAction(new ActionHandle(actionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Download folder",
                            ToolTip = "Dowload folder",
                            Icon = new ResourceHandle("Composite.Icons", "down"),
                            ActionLocation = _actionLocation
                        }
                    };
                }
            }

            var mediaRootEntityToken = entityToken as MediaRootFolderProviderEntityToken;
            if (mediaRootEntityToken != null)
            {
                var actionToken = new DownloadArchiveActionToken(mediaRootEntityToken.Id);

                yield return new ElementAction(new ActionHandle(actionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Download media archive",
                        ToolTip = "Dowload media archive",
                        Icon = new ResourceHandle("Composite.Icons", "down"),
                        ActionLocation = _actionLocation
                    }
                };
            }
        }
    }
}