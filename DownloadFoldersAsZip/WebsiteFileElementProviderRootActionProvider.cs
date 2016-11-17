using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Plugins.Elements.ElementProviders.WebsiteFileElementProvider;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [Export(typeof(IElementActionProviderFor))]
    public class WebsiteFileElementProviderRootActionProvider : IElementActionProviderFor
    {
        public IEnumerable<Type> ProviderFor
        {
            get { return new[] { typeof(WebsiteFileElementProviderRootEntityToken) }; }
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var websiteFileRootEntityToken = (WebsiteFileElementProviderRootEntityToken)entityToken;
            var actionToken = new DownloadActionToken("File", "/");

            yield return Actions.CreateElementAction(actionToken);

        }
    }
}
