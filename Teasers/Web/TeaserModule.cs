using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using Composite.Core.WebClient.Renderings.Page;
using Composite.Data;

using CompositeC1Contrib.Teasers.Data;
using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.Web
{
    public class TeaserModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.PostMapRequestHandler += app_PostMapRequestHandler;
        }

        public void Dispose() { }

        private static void app_PostMapRequestHandler(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var ctx = app.Context;

            var handler = ctx.CurrentHandler as Page;
            if (handler != null)
            {
                handler.PreInit += HandlerPreInit;
            }
        }

        private static void HandlerPreInit(object sender, EventArgs e)
        {
            var iPage = PageRenderer.CurrentPage;
            if (iPage == null)
            {
                return;
            }

            using (var data = new DataConnection())
            {
                var pageTeasers = TeaserFacade.GetPageTeasers(iPage, String.Empty, true).Select(t => new PageTeaserWrapper(t, 0)).ToList();
                var hideAncestorTeasers = false;

                var teaserSettings = data.Get<IPageTeaserSettings>().FirstOrDefault(f => f.PageId == iPage.Id);
                if (teaserSettings != null)
                {
                    hideAncestorTeasers = teaserSettings.HideAncestorTeasers;
                }

                if (!hideAncestorTeasers)
                {
                    var pageNode = data.SitemapNavigator.CurrentPageNode;
                    var ancestorPageNodes = AncestorPageNodes(pageNode);
                    var descendingTeasers = TeaserFacade.GetPageTeasers().Where(f => f.ShowOnDescendants).ToList();

                    pageTeasers.AddRange(ancestorPageNodes
                        .Join(descendingTeasers, a => a.Id, t => t.PageId, (a, t) => new PageTeaserWrapper(t, pageNode.Level - a.Level)).ToList());
                }

                foreach (var teaser in pageTeasers.Where(f => f.InterfaceType != typeof(IPageTeaserShared) && f.InterfaceType != typeof(IPageTeaserRandom)))
                {
                    TeaserFacade.PageTeasersForRequest.Add(new PageTeaserHolder(teaser));
                }

                var sharedTeasersInUse = new List<Guid>();

                foreach (var teaser in pageTeasers.Where(f => f.InterfaceType == typeof(IPageTeaserShared)))
                {
                    var pageTeaser = teaser.Teaser;
                    var sharedTeaserId = ((IPageTeaserShared)pageTeaser).SharedTeaserId;

                    if (sharedTeasersInUse.Contains(sharedTeaserId))
                    {
                        continue;
                    }

                    sharedTeasersInUse.Add(sharedTeaserId);

                    TeaserFacade.PageTeasersForRequest.Add(new PageTeaserHolder(pageTeaser.Position, sharedTeaserId, teaser));
                }

                //IPageTeaserRandom is transformed into IPageTeaserShared
                foreach (var teaser in pageTeasers.Where(f => f.InterfaceType == typeof(IPageTeaserRandom)))
                {
                    var pageTeaser = (IPageTeaserRandom)teaser.Teaser;
                    var teaserGroup = pageTeaser.TeaserGroup;

                    var sharedTeasers = TeaserFacade.GetSharedTeasers(teaserGroup).Where(t => t.InclInRotation && !sharedTeasersInUse.Contains(t.Id)).ToList();
                    if (!sharedTeasers.Any())
                    {
                        continue;
                    }

                    var rand = new Random();
                    var index = rand.Next(sharedTeasers.Count());
                    var sharedTeaser = sharedTeasers[index];

                    sharedTeasersInUse.Add(sharedTeaser.Id);

                    var pageTeaserShared = data.CreateNew<IPageTeaserShared>();

                    pageTeaserShared.PageId = pageTeaser.PageId;
                    pageTeaserShared.Name = sharedTeaser.Name;
                    pageTeaserShared.LocalOrdering = pageTeaser.LocalOrdering;
                    pageTeaserShared.Position = pageTeaser.Position;
                    pageTeaserShared.AdditionalHeader = pageTeaser.AdditionalHeader;

                    pageTeaserShared.SharedTeaserType = sharedTeaser.DataSourceId.InterfaceType.AssemblyQualifiedName;
                    pageTeaserShared.SharedTeaserId = sharedTeaser.Id;

                    var specificSharedTeaser = new PageTeaserWrapper(pageTeaserShared, 0);
                    var teaserHolder = new PageTeaserHolder(pageTeaser.Position, sharedTeaser.Id, specificSharedTeaser);

                    TeaserFacade.PageTeasersForRequest.Add(teaserHolder);
                }
            }
        }

        private static IEnumerable<PageNode> AncestorPageNodes(PageNode pageNode)
        {
            var ancestors = new List<PageNode>();

            while (pageNode.ParentPage != null)
            {
                pageNode = pageNode.ParentPage; // crawl up
                ancestors.Add(pageNode);
            }

            return ancestors;
        }
    }
}