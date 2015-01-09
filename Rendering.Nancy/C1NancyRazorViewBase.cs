using System;
using System.Threading;
using System.Xml.Linq;

using Composite.Data;

using Nancy.ViewEngines.Razor;

namespace CompositeC1Contrib.Rendering.Nancy
{
    public abstract class C1NancyRazorViewBase<T> : NancyRazorViewBase<T>
    {
        private bool _disposed;

        private DataConnection _data;
        public DataConnection Data
        {
            get { return _data ?? (_data = new DataConnection()); }
        }

        public SitemapNavigator Sitemap
        {
            get { return Data.SitemapNavigator; }
        }

        public PageNode CurrentPageNode
        {
            get { return Sitemap.CurrentPageNode; }
        }

        public PageNode HomePageNode
        {
            get { return Sitemap.CurrentHomePageNode; }
        }

        public string Lang
        {
            get { return Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName; }
        }

        public IHtmlString PageTemplateFeature(string featureName)
        {
            return Html.C1().GetPageTemplateFeature(featureName);
        }

        public IHtmlString Markup(XNode xNode)
        {
            return Html.C1().Markup(xNode);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _data.Dispose();
            }

            _disposed = true;
        }
    }

    public abstract class C1NancyRazorViewBase : C1NancyRazorViewBase<dynamic> { }
}
