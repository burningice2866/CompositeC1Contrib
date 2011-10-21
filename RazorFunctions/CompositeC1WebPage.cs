using System;
using System.Web.WebPages;

using Composite.Data;

namespace CompositeC1Contrib.RazorFunctions
{
    public abstract class CompositeC1WebPage : WebPage, IDisposable
    {
        private bool _disposed = false;
        private DataConnection _data;

        public DataConnection Data
        {
            get
            {
                if (_data == null)
                {
                    _data = new DataConnection();
                }

                return _data;
            }
        }

        public SitemapNavigator Sitemap
        {
            get { return Data.SitemapNavigator; }
        }

        public PageNode HomePageNode
        {
            get { return Sitemap.CurrentHomePageNode; }
        }

        public PageNode CurrentPageNode
        {
            get { return Sitemap.CurrentPageNode; }
        }

        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _data.Dispose();
                }

                _disposed = true;
            }
        }

        ~CompositeC1WebPage()
        {
            Dispose(false);
        }
    }
}
