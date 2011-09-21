using Composite.Data;

namespace CompositeC1Contrib.Web
{
    public class CompositeC1SiteMapNode : BaseSiteMapNode
    {
        public PageNode PageNode { get; protected set; }

        public CompositeC1SiteMapNode(CompositeC1SiteMapProvider provider, PageNode node, DataConnection data)
            : base(provider, node.Id.ToString(), data.CurrentLocale)
        {
            Title = node.MenuTitle;
            Description = node.Description;
            Url = node.Url;

            DocumentTitle = node.Title;
            Depth = node.Level;
            LastModified = node.ChangedDate;
            Priority = 5;

            PageNode = node;
        }
    }
}
