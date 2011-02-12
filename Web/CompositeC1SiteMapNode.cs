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
            Description = node.Description;
            Url = fixUrl(node.Url, data);

            DocumentTitle = node.Title;
            Depth = node.Level;
            LastModified = data.Get<IPage>().Single(p => p.Id == node.Id).ChangeDate;
            Priority = 5;

            PageNode = node;
        }

        private string fixUrl(string url, DataConnection data)
        {
            url = url.Replace(".aspx", String.Empty);

            int index = url.IndexOf("?");
            if (index > -1)
            {
                string query = url.Substring(index, url.Length - index);
                url = url.Substring(0, index);
            }

            var numberOfLocales = data.Get<ISystemActiveLocale>().Count();
            if (numberOfLocales == 1)
            {
                int secondSlash = url.IndexOf("/", 1);
                url = url.Remove(0, secondSlash == -1 ? url.Length : secondSlash);
            }
            else
            {
                var parts = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                url = parts[0];

                if (parts.Length > 1)
                {
                    int partsToSkip = data.CurrentPublicationScope == PublicationScope.Published ? 2 : 1;
                    url = parts[0] + "/" + String.Join("/", parts.Skip(partsToSkip));
                }
            }            

            url = UrlUtils.GetCleanUrl(url);

            if (!url.StartsWith("/"))
            {
                url = "/" + url;
            }

            if (data.CurrentPublicationScope == PublicationScope.Unpublished)
            {
                url += "?dataScope=administrated";
            }

            return url;
        }
    }
}
