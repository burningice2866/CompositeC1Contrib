using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace CompositeC1Contrib.Web
{
    public abstract class BaseSiteMapProvider : SiteMapProvider
    {
        protected class SiteMapContainer
        {
            public SiteMapNode Root { get; set; }

            public IDictionary<string, SiteMapNode> KeyToNodesMap { get; private set; }
            public IDictionary<string, SiteMapNode> RawUrlToNodesMap { get; private set; }
            public IDictionary<string, SiteMapNode> ParentNodesMap { get; private set; }
            public IDictionary<string, SiteMapNodeCollection> ChildCollectionsMap { get; private set; }

            public SiteMapContainer()
            {
                KeyToNodesMap = new Dictionary<string, SiteMapNode>();
                RawUrlToNodesMap = new Dictionary<string, SiteMapNode>();
                ParentNodesMap = new Dictionary<string, SiteMapNode>();
                ChildCollectionsMap = new Dictionary<string, SiteMapNodeCollection>();
            }
        }

        private const string _key = "sitemap";
        private static readonly object _lock = new object();

        private DateTime? _lastRefreshedTime;

        protected abstract bool CanCache { get; }

        public bool ExtranetEnabled { get; private set; }
        public TimeSpan? AutoRefreshInterval { get; private set; }

        public override SiteMapProvider ParentProvider { get; set; }

        public override SiteMapNode CurrentNode
        {
            get { return FindSiteMapNode(HttpContext.Current); }
        }

        public override SiteMapNode RootNode
        {
            get { return GetRootNodeCore(); }
        }

        public override SiteMapProvider RootProvider
        {
            get
            {
                if (ParentProvider != null)
                {
                    return ParentProvider.RootProvider;
                }

                return this;
            }
        }

        public override SiteMapNode FindSiteMapNode(HttpContext ctx)
        {
            string key = GetCurrentKey();

            return FindSiteMapNodeFromKey(key);
        }

        public override SiteMapNode FindSiteMapNode(string rawUrl)
        {
            return FindSiteMapNode(rawUrl, CultureInfo.CurrentCulture);
        }

        public SiteMapNode FindSiteMapNode(string rawUrl, CultureInfo ci)
        {
            SiteMapNode node = null;
            var container = GetContainer(ci);
            container.RawUrlToNodesMap.TryGetValue(rawUrl, out node);

            return node;
        }

        public override SiteMapNode FindSiteMapNodeFromKey(string key)
        {
            var node = FindSiteMapNodeFromKey(key, false);
            if (node == null)
            {
                node = FindSiteMapNodeFromKey(key, true);
            }

            return node;
        }

        public SiteMapNode FindSiteMapNodeFromKey(string key, bool allLanguages)
        {
            if (!allLanguages)
            {
                return FindSiteMapNodeFromKey(key, CultureInfo.CurrentCulture);
            }

            var list = loadSiteMap();
            foreach (var container in list.Values)
            {
                SiteMapNode node = null;
                if (container.KeyToNodesMap.TryGetValue(key, out node))
                {
                    return node;
                }
            }

            return null;
        }

        public SiteMapNode FindSiteMapNodeFromKey(string key, CultureInfo ci)
        {
            SiteMapNode node = null;
            var container = GetContainer(ci);

            container.KeyToNodesMap.TryGetValue(key, out node);

            return node;
        }

        public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            SiteMapNodeCollection childNodes = null;
            var ci = ((BaseSiteMapNode)node).Culture;
            var container = GetContainer(ci);

            container.ChildCollectionsMap.TryGetValue(node.Key, out childNodes);

            if (childNodes == null)
            {
                return SiteMapNodeCollection.ReadOnly(new SiteMapNodeCollection());
            }

            if (!SecurityTrimmingEnabled)
            {
                return SiteMapNodeCollection.ReadOnly(childNodes);
            }

            var ctx = HttpContext.Current;
            var returnList = new SiteMapNodeCollection(childNodes.Count);

            foreach (SiteMapNode child in childNodes)
            {
                if (child.IsAccessibleToUser(ctx))
                {
                    returnList.Add(child);
                }
            }

            return SiteMapNodeCollection.ReadOnly(returnList);
        }

        public override SiteMapNode GetParentNode(SiteMapNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            SiteMapNode parentNode = null;
            var ci = ((BaseSiteMapNode)node).Culture;
            var container = GetContainer(ci);

            container.ParentNodesMap.TryGetValue(node.Key, out parentNode);

            if ((parentNode == null) && (ParentProvider != null))
            {
                parentNode = ParentProvider.GetParentNode(node);
            }

            if (parentNode == null) return null;
            if (!parentNode.IsAccessibleToUser(HttpContext.Current)) return null;

            return parentNode;
        }

        protected override SiteMapNode GetRootNodeCore()
        {
            var container = GetContainer(CultureInfo.CurrentCulture);

            return container.Root;
        }

        public override void Initialize(string name, NameValueCollection attributes)
        {
            if (attributes != null)
            {
                bool extranetEnabled = false;
                if (bool.TryParse(attributes["extranetEnabled"], out extranetEnabled))
                {
                    ExtranetEnabled = extranetEnabled;
                }

                attributes.Remove("extranetEnabled");

                int interval = 0;
                if (int.TryParse(attributes["autoRefreshInterval"], out interval))
                {
                    AutoRefreshInterval = new TimeSpan(0, 0, interval);
                }

                attributes.Remove("autoRefreshInterval");
            }

            base.Initialize(name, attributes);
        }

        public IEnumerable<BaseSiteMapNode> GetRootNodes()
        {
            var list = new List<SiteMapNode>();
            foreach (var container in loadSiteMap())
            {
                var node = container.Value.Root;
                if (node != null)
                {
                    list.Add(node);
                }
            }

            return list.Cast<BaseSiteMapNode>();
        }

        public void Flush()
        {
            var keys = MemoryCache.Default.Where(o => o.Key.StartsWith(_key)).Select(o => o.Key);
            foreach (var key in keys)
            {
                MemoryCache.Default.Remove(key);
            }
        }

        protected void AddNode(SiteMapNode node, SiteMapNode parentNode, SiteMapContainer container)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (container == null) throw new ArgumentNullException("container");

            container.KeyToNodesMap.Add(node.Key, node);
            container.ParentNodesMap.Add(node.Key, parentNode);

            if (!container.RawUrlToNodesMap.ContainsKey(node.Url))
            {
                container.RawUrlToNodesMap.Add(node.Url, node);
            }

            if (parentNode != null)
            {
                if (!container.ChildCollectionsMap.ContainsKey(parentNode.Key))
                {
                    container.ChildCollectionsMap[parentNode.Key] = new SiteMapNodeCollection();
                }

                container.ChildCollectionsMap[parentNode.Key].Add(node);
            }
        }

        private SiteMapContainer GetContainer(CultureInfo ci)
        {
            var containers = loadSiteMap();            
            return containers[ci];
        }

        private IDictionary<CultureInfo, SiteMapContainer> loadSiteMap()
        {
            string host = HttpContext.Current.Request.Url.Host;
            bool forceRefresh = AutoRefreshInterval.HasValue
                && _lastRefreshedTime.HasValue
                && DateTime.Now - _lastRefreshedTime > AutoRefreshInterval;

            IDictionary<CultureInfo, SiteMapContainer> list = null;
            if (forceRefresh || ((list = loadFromCache(host)) == null))
            {
                lock (_lock)
                {
                    if (list == null)
                    {
                        list = new Dictionary<CultureInfo, SiteMapContainer>();

                        LoadSiteMapInternal(list, host);
                        AddRolesInternal(list);

                        addToCache(list, host);
                    }
                }
            }

            return list;
        }

        private IDictionary<CultureInfo, SiteMapContainer> loadFromCache(string host)
        {
            var ctx = HttpContext.Current;
            var key = _key + host;

            var container = ctx.Items[key] as IDictionary<CultureInfo, SiteMapContainer>;
            if (container == null)
            {
                if (!CanCache) return null;

                container = MemoryCache.Default.Get(key) as IDictionary<CultureInfo, SiteMapContainer>;
                if (container != null)
                {
                    ctx.Items.Add(key, container);
                }
            }

            return container;
        }

        private void addToCache(IDictionary<CultureInfo, SiteMapContainer> container, string host)
        {
            var ctx = HttpContext.Current;
            var key = _key + host;

            ctx.Items[key] = container;

            if (CanCache)
            {
                _lastRefreshedTime = DateTime.Now;
                MemoryCache.Default.Add(key, container, ObjectCache.InfiniteAbsoluteExpiration);
            }
        }

        protected abstract string GetCurrentKey();
        protected abstract void LoadSiteMapInternal(IDictionary<CultureInfo, SiteMapContainer> list, string host);
        protected abstract void AddRolesInternal(IDictionary<CultureInfo, SiteMapContainer> list);
    }
}
