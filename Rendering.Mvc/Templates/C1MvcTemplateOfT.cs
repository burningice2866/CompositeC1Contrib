using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

using Composite.Data;

namespace CompositeC1Contrib.Rendering.Mvc.Templates
{
    public abstract class C1MvcTemplate<T> : WebViewPage<T>
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

        public IHtmlString PageTemplateFeature(string featureName)
        {
            return Html.C1().GetPageTemplateFeature(featureName);
        }

        public IHtmlString Markup(XNode xNode)
        {
            return Html.C1().Markup(xNode);
        }

        public override void ExecutePageHierarchy()
        {
            ViewContext.TempData["HtmlHelper"] = Html;

            base.ExecutePageHierarchy();
        }
    }
}
