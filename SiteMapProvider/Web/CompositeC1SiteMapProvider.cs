using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;

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
                var overridenContext = SiteMapContext.OverrideContext;
                if (overridenContext != null)
                {
                    return overridenContext.PublicationScope;
                }

                return DataScopeManager.CurrentDataScope.ToPublicationScope();
            }
        }

        protected override bool CanCache
        {
            get
            {
                var scope = DataScopeManager.CurrentDataScope.ToPublicationScope();
                if (scope == PublicationScope.Unpublished)
                {
                    return false;
        }

                return true;
            }
        }

        protected override string GetRequestKey()
        {
            return PublicationScope.ToString();
        }

        protected override string GetCurrentNodeKey()
        {
            return SitemapNavigator.CurrentPageId.ToString();
        }

        public override SiteMapNode FindSiteMapNode(string rawUrl)
        {
            var ci = DataLocalizationFacade.ActiveLocalizationCultures.SingleOrDefault(c => rawUrl.StartsWith("/" + c.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));
            if (ci == null)
            {
                ci = DataLocalizationFacade.DefaultLocalizationCulture;
            }

            var node = base.FindSiteMapNode(rawUrl, ci) as CompositeC1SiteMapNode;
            if (node == null)
            {
                if (UrlUtils.IsDefaultDocumentUrl(rawUrl))
                {
                    node = RootNode as CompositeC1SiteMapNode;
                }
            }

            return node ?? base.FindSiteMapNode(rawUrl);
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

            DataEventSystemFacade.SubscribeToDataAfterAdd<IPage>(handler, false);
            DataEventSystemFacade.SubscribeToDataAfterUpdate<IPage>(handler, false);
            DataEventSystemFacade.SubscribeToDataDeleted<IPage>(handler, false);

            DataEventSystemFacade.SubscribeToDataAfterAdd<IPageStructure>(handler, false);
            DataEventSystemFacade.SubscribeToDataAfterUpdate<IPageStructure>(handler, false);
            DataEventSystemFacade.SubscribeToDataDeleted<IPageStructure>(handler, false);

            DataEventSystemFacade.SubscribeToDataAfterAdd<ISystemActiveLocale>(handler, false);
            DataEventSystemFacade.SubscribeToDataAfterUpdate<ISystemActiveLocale>(handler, false);
            DataEventSystemFacade.SubscribeToDataDeleted<ISystemActiveLocale>(handler, false);

            base.Initialize(name, attributes);
        }

        protected override void LoadSiteMapInternal(IDictionary<CultureInfo, SiteMapContainer> list, string host)
        {
            var scope = PublicationScope;

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

        protected override Uri ProcessUrl(Uri uri)
        {
            PageUrlData urlData;
            
            var ctx = HttpContext.Current;

            string _previewKey = ctx.Request.QueryString["previewKey"];
            if (!String.IsNullOrEmpty(_previewKey))
            {
                var page = (IPage)ctx.Cache.Get(_previewKey + "_SelectedPage");
                urlData = new PageUrlData(page);
            }
            else
            {
                urlData = PageUrls.ParseUrl(ctx.Request.Url.ToString());
            }

            if (urlData != null)
            {
                var publicUrl = PageUrls.BuildUrl(urlData, UrlKind.Public, new UrlSpace() { Hostname = uri.Host, ForceRelativeUrls = false });
                if (Uri.IsWellFormedUriString(publicUrl, UriKind.Absolute))
                {
                    var newHost = new Uri(publicUrl).Host;

                    uri = new Uri(uri.ToString().Replace(uri.Host, newHost));
                }
            }

            var overridenContext = SiteMapContext.OverrideContext;
            if (overridenContext != null)
            {
                uri = new Uri(uri.ToString().Replace(uri.Host, overridenContext.Host));
            }

            return uri;            
        }

        protected override void AddRolesInternal(IDictionary<CultureInfo, SiteMapContainer> list) { }

        private void loadNodes(PageNode pageNode, SiteMapNode parent, SiteMapContainer container, DataConnection data)
        {
            if (pageNode.Url == null)
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
    }
}
