using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;

using Composite.C1Console.Security;
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
                if ((HttpContext.Current.Request.QueryString["dataScope"] == "administrated" && UserValidationFacade.IsLoggedIn())
                    || RequestInfo.Current.IsPreview)
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
            return SitemapNavigator.CurrentPageId.ToString();
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

        protected override void LoadSiteMapInternal(IDictionary<CultureInfo, SiteMapContainer> list, string host)
        {
            var scope = PublicationScope;

            if (scope == PublicationScope.Unpublished)
            {
                host = ensureCorrectHost(host);
            }

            foreach (var ci in DataLocalizationFacade.ActiveLocalizationCultures)
            {
                using (var data = new DataConnection(scope, ci))
                {
                    var root = data.SitemapNavigator.GetPageNodeByHostname(host);
                    if (root != null)
                    {
                        var container = new SiteMapContainer()
                        {
                            Root = new CompositeC1SiteMapNode(this, root, data)
                        };

                        list.Add(ci, container);

                        loadNodes(root, null, container, data);
                    }
                }
            }
        }

        protected override void AddRolesInternal(IDictionary<CultureInfo, SiteMapContainer> list) { }

        private string ensureCorrectHost(string host)
        {
            using (var data = new DataConnection())
            {
                var websites = data.SitemapNavigator.HomePageIds;
                if (websites.Count() > 1)
                {
                    IPage page = null;

                    var currentNode = data.SitemapNavigator.CurrentHomePageNode;
                    if (currentNode != null)
                    {
                        page = data.Get<IPage>().SingleOrDefault(p => p.Id == currentNode.Id);
                    }
                    
                    if (page == null)
                    {
                        var path = HttpContext.Current.Request.Url.LocalPath;
                        int secondSlash;

                        if (data.Get<ISystemActiveLocale>().Count() > 1)
                        {
                            secondSlash = path.IndexOf("/", 1);
                            path = path.Remove(0, secondSlash == -1 ? path.Length : secondSlash);
                        }

                        secondSlash = path.IndexOf("/", 1);
                        var currentWebsite = path.Substring(1, (secondSlash == -1 ? path.Length : secondSlash) - 1).Replace(".aspx", String.Empty);

                        page = websites
                            .Select(site => data.Get<IPage>().Single(p => p.Id == site))
                            .SingleOrDefault(p => p.UrlTitle.Equals(currentWebsite, StringComparison.OrdinalIgnoreCase));
                    }

                    if (page != null)
                    {
                        var hostNameBinding = data.Get<IPageHostNameBinding>().SingleOrDefault(p => p.PageId == page.Id);
                        if (hostNameBinding != null)
                        {
                            return hostNameBinding.HostName;
                        }
                    }
                }
            }

            return host;
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
    }
}
