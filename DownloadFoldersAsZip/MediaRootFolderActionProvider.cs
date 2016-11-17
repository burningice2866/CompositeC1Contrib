using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Plugins.Elements.ElementProviders.MediaFileProviderElementProvider;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [Export(typeof(IElementActionProviderFor))]
    public class MediaRootFolderActionProvider : IElementActionProviderFor
    {
        public IEnumerable<Type> ProviderFor
        {
            get { return new[] { typeof(MediaRootFolderProviderEntityToken) }; }
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var mediaRootEntityToken = (MediaRootFolderProviderEntityToken)entityToken;
            var actionToken = new DownloadActionToken("MediaArchive", mediaRootEntityToken.Id);

            yield return Actions.CreateElementAction(actionToken);
        }
    }
}
