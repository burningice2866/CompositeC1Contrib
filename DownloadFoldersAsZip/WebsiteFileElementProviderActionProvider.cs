using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Web.Hosting;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Plugins.Elements.ElementProviders.WebsiteFileElementProvider;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [Export(typeof(IElementActionProviderFor))]
    public class WebsiteFileElementProviderActionProvider : IElementActionProviderFor
    {
        public IEnumerable<Type> ProviderFor
        {
            get { return new[] { typeof(WebsiteFileElementProviderEntityToken) }; }
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var websiteFileEntityToken = (WebsiteFileElementProviderEntityToken)entityToken;

            var attr = File.GetAttributes(websiteFileEntityToken.Path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var root = HostingEnvironment.MapPath("~");
                var relativePath = websiteFileEntityToken.Path.Remove(0, root.Length);

                var actionToken = new DownloadActionToken("File", "/" + relativePath);

                yield return Actions.CreateElementAction(actionToken);
            }
        }
    }
}
