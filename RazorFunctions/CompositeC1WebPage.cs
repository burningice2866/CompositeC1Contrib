using System.Web.WebPages;

using Composite.Data;

namespace CompositeC1Contrib.RazorFunctions
{
    public abstract class CompositeC1WebPage : WebPage
    {
        public DataConnection Data
        {
            get { return new DataConnection(); }
        }

        public PageNode HomePageNode
        {
            get
            {
                using (var data = Data)
                {
                    return data.SitemapNavigator.CurrentHomePageNode;
                }
            }
        }

        public PageNode CurrentPageNode
        {
            get
            {
                using (var data = Data)
                {
                    return data.SitemapNavigator.CurrentPageNode;
                }
            }
        }
    }
}
