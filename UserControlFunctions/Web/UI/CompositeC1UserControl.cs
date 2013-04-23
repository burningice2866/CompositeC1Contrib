using System;
using System.Web.UI;

using Composite.Data;

namespace CompositeC1Contrib.UserControlFunctions.Web.UI
{
    public class CompositeC1UserControl : UserControl
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

        ~CompositeC1UserControl()
        {
            Dispose(false);
        }
    }
}
