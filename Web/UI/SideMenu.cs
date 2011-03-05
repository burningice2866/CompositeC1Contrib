using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace CompositeC1Contrib.Web.UI
{
    public class MenuItemEventArgs : EventArgs
    {
        public int Level { get; private set; }
        public HtmlAnchor Anchor { get; private set; }

        public MenuItemEventArgs(HtmlAnchor a, int level)
        {
            Anchor = a;
            Level = level;
        }
    }

    public class SideMenu : Control
    {
        public event EventHandler<MenuItemEventArgs> MenuItemAdding;

        private SiteMapNode _rootNode = SiteMap.RootNode;
        public SiteMapNode RootNode
        {
            get { return this._rootNode; }
            set { this._rootNode = value; }
        }

        public SideMenu()
        {
            ID = "SideMenu";
        }

        protected override void CreateChildControls()
        {
            var div = new HtmlGenericControl("div");
            div.Attributes.Add("id", ID);
            Controls.Add(div);

            CreateMenuNodes(RootNode, div, 1);

            base.CreateChildControls();
        }

        private void CreateMenuNodes(SiteMapNode node, HtmlGenericControl container, int level)
        {
            if (!String.IsNullOrEmpty(node.Title) && node.HasChildNodes)
            {
                var ul = new HtmlGenericControl("ul");
                container.Controls.Add(ul);

                foreach (SiteMapNode child in node.ChildNodes)
                {
                    var li = new HtmlGenericControl("li");
                    ul.Controls.Add(li);

                    var a = new HtmlAnchor()
                    {
                        InnerHtml = HttpUtility.HtmlEncode(child.Title),
                        Title = child.Title,
                        HRef = child.Url
                    };

                    OnMenuItemAdding(new MenuItemEventArgs(a, level));

                    li.Controls.Add(a);

                    if (SiteMap.CurrentNode.IsEqualToOrDescendantOf(child))
                    {
                        li.Attributes["class"] = "selected";

                        CreateMenuNodes(child, li, (level + 1));
                    }
                }
            }
        }

        protected void OnMenuItemAdding(MenuItemEventArgs e)
        {
            if (MenuItemAdding != null)
            {
                MenuItemAdding(this, e);
            }
        }
    }
}
