using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [Export(typeof(IElementActionProviderFor))]
    public class DataActionProvider : IElementActionProviderFor
    {
        public IEnumerable<Type> ProviderFor
        {
            get { return new[] { typeof(DataEntityToken) }; }
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var dataEntityToken = (DataEntityToken)entityToken;

            var folder = dataEntityToken.Data as IMediaFileFolder;
            if (folder != null)
            {
                var actionToken = new DownloadActionToken("MediaFolder", folder.KeyPath);

                yield return Actions.CreateElementAction(actionToken);
            }
        }
    }
}
