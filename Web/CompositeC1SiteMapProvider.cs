using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;

using Composite.Core.WebClient.Renderings.Page;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web
{
    public class CompositeC1SiteMapProvider : BaseSiteMapProvider
    {
        protected override bool CanCache
        {
            get
            {
                var pageUrl = RequestInfo.Current.PageUrl;
                if (pageUrl == null)
                {
                    return false;
                }

                return pageUrl.PublicationScope != PublicationScope.Published;
            }
        }

        protected override string GetCurrentKey()
        {
            return PageRenderer.CurrentPage.Id.ToString();
        }

        public override bool IsAccessibleToUser(HttpContext ctx, SiteMapNode node)
        {
            var pageUrl = RequestInfo.Current.PageUrl;
            if (pageUrl != null && pageUrl.PublicationScope == PublicationScope.Unpublished)
            {
                return true;
            }

            if (ExtranetEnabled)
            {
                throw new NotImplementedException("Extranet is not implemented in the currect C1 sitemap provider");
            }

            return true;
        }

        protected override void LoadSiteMapInternal(IDictionary<CultureInfo, SiteMapContainer> list)
        {
            foreach (var ci in DataLocalizationFacade.ActiveLocalizationCultures)
            {
                using (var data = new DataConnection(ci))
                {
                    var host = HttpContext.Current.Request.Url.Host;

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

        public static void DataBeforeAdd(object sender, DataEventArgs e)
        {
            var page = e.GetData<IPage>();
            page.UrlTitle = UrlUtils.GetCleanUrl(page.UrlTitle);
        }
    }
}
