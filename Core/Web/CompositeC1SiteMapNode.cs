using System;
using System.Linq;

using Composite.Data;
using Composite.Data.Types;

using C1UrlUtils = Composite.Core.WebClient.UrlUtils;

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
            if (AppSettings.UseBetterUrls)
            {
                url = url.Replace(".aspx", String.Empty);

                if (!String.IsNullOrEmpty(C1UrlUtils.PublicRootPath))
                {
                    url = url.Remove(0, C1UrlUtils.PublicRootPath.Length);
                }

                var localeMapping = DataLocalizationFacade.GetUrlMappingName(data.CurrentLocale);
                if (!String.IsNullOrEmpty(localeMapping))
                {
                    url = url.Remove(0, localeMapping.Length + 1);
                }

                int index = url.IndexOf("?");
                if (index > -1)
                {
                    string query = url.Substring(index, url.Length - index);
                    url = url.Substring(0, index);
                }

                int secondSlash = url.IndexOf("/", 1);
                url = url.Remove(0, secondSlash == -1 ? url.Length : secondSlash);

                url = UrlUtils.GetCleanUrl(url);

                if (!AppSettings.UseExtensionlessUrls && !String.IsNullOrEmpty(url))
                {
                    url += ".aspx";
                }

                if (!String.IsNullOrEmpty(C1UrlUtils.PublicRootPath))
                {
                    url = C1UrlUtils.PublicRootPath + url;
                }

                if (!String.IsNullOrEmpty(localeMapping))
                {
                    url = "/" + localeMapping + url;
                }

                if (!url.StartsWith("/"))
                {
                    url = "/" + url;
                }

                if (data.CurrentPublicationScope == PublicationScope.Unpublished)
                {
                    url += "?dataScope=administrated";
                }
            }

            return url;
        }
    }
}
