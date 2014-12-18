using System.Web.Mvc;

using Composite.Data;

namespace CompositeC1Contrib.Rendering.Mvc
{
    public abstract class C1MvcTemplate : WebViewPage
    {
        private DataConnection _dataConnection;

        public DataConnection Data
        {
            get { return _dataConnection ?? (_dataConnection = new DataConnection()); }
        }

        public SitemapNavigator SitemapNavigator
        {
            get { return Data.SitemapNavigator; }
        }
    }
}
