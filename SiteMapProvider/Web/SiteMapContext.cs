using System;
using System.Linq;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web
{
    public class SiteMapContext : IDisposable
    {
        private static SiteMapContext _overridenContext = null;
        public static SiteMapContext OverrideContext
        {
            get { return _overridenContext; }
        }

        public string Host { get; private set; }
        public PublicationScope PublicationScope { get; private set; }

        private SiteMapContext() { }

        public SiteMapContext(string rootTitle, PublicationScope scope)
        {
            using (var data = new DataConnection())
            {
                var sitemap = data.SitemapNavigator;
                var root = sitemap.HomePageNodes.SingleOrDefault(n => n.Title == rootTitle);

                if (root != null)
                {
                    var binding = data.Get<IHostnameBinding>().FirstOrDefault(b => b.HomePageId == root.Id);
                    if (binding != null)
                    {
                        _overridenContext = new SiteMapContext()
                        {
                            Host = binding.Hostname,
                            PublicationScope = scope
                        };
                    }
                }
            }
        }

        public void Dispose()
        {
            _overridenContext = null;
        }
    }
}
