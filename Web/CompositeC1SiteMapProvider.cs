using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;

using Composite.C1Console.Security;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web
{
    public class CompositeC1SiteMapProvider : BaseSiteMapProvider
    {
        private PublicationScope PublicationScope
        {
            get
            {
                if ((HttpContext.Current.Request.QueryString["dataScope"] == "administrated" || RequestInfo.Current.IsPreview)
                    && UserValidationFacade.IsLoggedIn())
                {
                    return PublicationScope.Unpublished;
                }

                return PublicationScope.Published;
            }
        }

        protected override bool CanCache
        {
            get { return PublicationScope == PublicationScope.Published; }
        }

        protected override string GetCurrentKey()
        {
            return PageRenderer.CurrentPage.Id.ToString();
        }

        public override bool IsAccessibleToUser(HttpContext ctx, SiteMapNode node)
        {
            if (PublicationScope == PublicationScope.Unpublished)
            {
                return true;
            }

            if (ExtranetEnabled)
            {
                throw new NotImplementedException("Extranet is not implemented in the currect C1 sitemap provider");
            }

            return true;
        }

        protected override void LoadSiteMapInternal(IDictionary<CultureInfo, SiteMapContainer> list, string host)
        {
            if (PublicationScope == PublicationScope.Unpublished)
            {
                var path = HttpContext.Current.Request.Url.LocalPath;

                using (var data = new DataConnection())
                {
                    var numberOfLocales = data.Get<ISystemActiveLocale>().Count();
                    if (numberOfLocales > 1)
                    {
                        int secondSlash = path.IndexOf("/", 1);
                        path = path.Remove(0, secondSlash == -1 ? path.Length : secondSlash);
                    }

                    var websites = data.Get<IPageStructure>().Where(s => s.ParentId == Guid.Empty);
                    if (websites.Count() > 1)
                    {
                        int secondSlash = path.IndexOf("/", 1);
                        var currentWebsite = path.Substring(1, (secondSlash == -1 ? path.Length : secondSlash) - 1);

                        var page = websites
                            .Select(s => data.Get<IPage>().Single(p => p.Id == s.Id))
                            .Single(p => p.UrlTitle.Equals(currentWebsite, StringComparison.OrdinalIgnoreCase));

                        var hostNameBinding = data.Get<IPageHostNameBinding>().SingleOrDefault(p => p.PageId == page.Id);
                        if (hostNameBinding != null)
                        {
                            host = hostNameBinding.HostName;
                        }
                    }
                }
            }

            foreach (var ci in DataLocalizationFacade.ActiveLocalizationCultures)
            {
                using (var data = new DataConnection(PublicationScope, ci))
                {
                    var root = data.SitemapNavigator.GetPageNodeByHostname(host);
                    if (root != null)
                    {
                        var container = new SiteMapContainer() { Root = new CompositeC1SiteMapNode(this, root, data) };
                        list.Add(ci, container);

                        loadNodes(root, null, container, data);
                    }
                }
            }
        }

        protected override void AddRolesInternal(IDictionary<CultureInfo, SiteMapContainer> list) { }

        public override SiteMapNode FindSiteMapNode(string rawUrl)
        {
            int index = rawUrl.IndexOf("?");
            if (index > -1)
            {
                string query = rawUrl.Substring(index, rawUrl.Length - index);
                var qs = HttpUtility.ParseQueryString(query);

                rawUrl = rawUrl.Substring(0, index);

                if (qs["dataScope"] == "administrated")
                {
                    rawUrl += "?dataScope=administrated";
                }
            }

            return base.FindSiteMapNode(rawUrl);
        }

        private void loadNodes(PageNode pageNode, SiteMapNode parent, SiteMapContainer container, DataConnection data)
        {
            var node = new CompositeC1SiteMapNode(this, pageNode, data);
            AddNode(node, parent, container);

            var childs = pageNode.ChildPages;
            foreach (var child in childs)
            {
                loadNodes(child, node, container, data);
            }
        }

        public override void Initialize(string name, NameValueCollection attributes)
        {
            DataEventHandler handler = (sender, e) => Flush();

            DataEventSystemFacade.SubscribeToDataAfterAdd<IPage>(handler);
            DataEventSystemFacade.SubscribeToDataAfterUpdate<IPage>(handler);
            DataEventSystemFacade.SubscribeToDataDeleted<IPage>(handler);

            DataEventSystemFacade.SubscribeToDataAfterAdd<IPageStructure>(handler);
            DataEventSystemFacade.SubscribeToDataAfterUpdate<IPageStructure>(handler);
            DataEventSystemFacade.SubscribeToDataDeleted<IPageStructure>(handler);

            DataEventSystemFacade.SubscribeToDataAfterAdd<ISystemActiveLocale>(handler);
            DataEventSystemFacade.SubscribeToDataAfterUpdate<ISystemActiveLocale>(handler);
            DataEventSystemFacade.SubscribeToDataDeleted<ISystemActiveLocale>(handler);

            base.Initialize(name, attributes);
        }
    }
}
