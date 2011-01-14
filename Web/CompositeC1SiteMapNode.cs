using System;
using System.Linq;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web
{
    public class CompositeC1SiteMapNode : BaseSiteMapNode
    {
        public PageNode PageNode { get; protected set; }

        public CompositeC1SiteMapNode(CompositeC1SiteMapProvider provider, PageNode node, DataConnection data)
            : base(provider, node.Id.ToString(), data.CurrentLocale)
        {
            Title = node.MenuTitle;
            DocumentTitle = node.Title;
            Description = node.Description;
            Url = fixUrl(node.Url);
            Depth = node.Level;
            LastModified = data.Get<IPage>().Single(p => p.Id == node.Id).ChangeDate;

            PageNode = node;
        }

        private string fixUrl(string url)
        {
            url = url.Replace(".aspx", String.Empty);
            url = removeLanguagePrefix(url);
            url = UrlUtils.GetCleanUrl(url);

            return url;
        }

        private string removeLanguagePrefix(string url)
        {
            foreach (var ci in DataLocalizationFacade.ActiveLocalizationCultures)
            {
                if (url.StartsWith("/" + ci.TwoLetterISOLanguageName))
                {
                    url = url.Remove(0, 3);
                }
            }

            return url;
        }
    }
}
