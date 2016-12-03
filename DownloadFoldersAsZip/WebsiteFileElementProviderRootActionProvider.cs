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
        public IEnumerable<Type> ProviderFor => new[] { typeof(WebsiteFileElementProviderRootEntityToken) };

        public void AddActions(Element element)
        {
            var actions = Provide(element.ElementHandle.EntityToken);

            element.AddAction(actions);
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var actionToken = new DownloadActionToken("File", "/");

            yield return Actions.CreateElementAction(actionToken);

        }
    }
}
