using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;

using Composite.C1Console.Security;
using Composite.Core.Routing;
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
                if ((HttpContext.Current.Request.Url.LocalPath.IndexOf("/c1mode(unpublished)") > -1 && UserValidationFacade.IsLoggedIn())
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

            var urlData = PageUrls.ParseUrl(HttpContext.Current.Request.Url.ToString());
            var publicUrl = PageUrls.BuildUrl(urlData, UrlKind.Public, new UrlSpace() { Hostname = host, ForceRelativeUrls = false });

            if (Uri.IsWellFormedUriString(publicUrl, UriKind.Absolute))
            {
                host = new Uri(publicUrl).Host;
            }

            foreach (var ci in DataLocalizationFacade.ActiveLocalizationCultures)
            {
                using (var data = new DataConnection(scope, ci))
                {
                    var rootPage = data.SitemapNavigator.GetPageNodeByHostname(host);

                    if (rootPage != null)
                    {
                        var container = new SiteMapContainer()
                        {
                            Root = new CompositeC1SiteMapNode(this, rootPage, data)
                        };

                        list.Add(ci, container);

                        loadNodes(rootPage, null, container, data);
                    }
                }
            }
        }

        protected override void AddRolesInternal(IDictionary<CultureInfo, SiteMapContainer> list) { }

        private void loadNodes(PageNode pageNode, SiteMapNode parent, SiteMapContainer container, DataConnection data)
        {
            try
            {
                string url = pageNode.Url;
            }
            catch (NullReferenceException)
            {
                return;
            }

            var node = new CompositeC1SiteMapNode(this, pageNode, data);
            AddNode(node, parent, container);

            var childs = pageNode.ChildPages;
            foreach (var child in childs)
            {
                loadNodes(child, node, container, data);
            }
        }

        public static CompositeC1SiteMapNode ResolveNodeFromUrl(string localPath, string query)
        {
            var provider = (CompositeC1SiteMapProvider)SiteMap.Provider;

            var ci = DataLocalizationFacade.ActiveLocalizationCultures.SingleOrDefault(c => localPath.StartsWith("/" + c.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));
            if (ci == null)
            {
                ci = DataLocalizationFacade.DefaultLocalizationCulture;
            }

            var node = provider.FindSiteMapNode(localPath, ci) as CompositeC1SiteMapNode;
            if (node == null)
            {
                if (UrlUtils.IsDefaultDocumentUrl(localPath))
                {
                    node = provider.RootNode as CompositeC1SiteMapNode;
                }
            }

            return node;
        }
    }
}
