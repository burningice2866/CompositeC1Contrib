using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using Composite.Data;

using CompositeC1Contrib.Sorting.Web.UI;
using CompositeC1Contrib.Teasers.Data;
using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.Web.UI
{
    public class SortPageTeasersPage : BaseSortPage 
    {
        [WebMethod]
        public static void UpdateOrder(string pageId, string position, string consoleId, string entityToken, string serializedOrder)
        {
            var page = PageManager.GetPageById(new Guid(pageId));
            var pageTeasers = TeaserFacade.GetPageTeasers(page, position, false).ToList();

            UpdateOrder(pageTeasers, serializedOrder);

            var serializedEntityToken = HttpUtility.UrlDecode(entityToken);
            if (!String.IsNullOrEmpty(serializedEntityToken))
            {
                UpdateParents(serializedEntityToken, consoleId);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            var pageId = Request.QueryString["pageId"];
            var position = Request.QueryString["position"];
            
            if (Request.HttpMethod == "POST")
            {
                pageId = Request.Form["pageId"];
                position = Request.Form["position"];
            }

            var page = PageManager.GetPageById(Guid.Parse(pageId));
            var pageTeasers = TeaserFacade.GetPageTeasers(page, position, false);

            Master.CustomJsonDataName = "position";
            Master.CustomJsonDataValue = position;

            Master.SortableItems = pageTeasers.Select(i => new SortableItem
            {
                Id = HashId(i),
                Name = i.GetLabel()
            });

            base.OnLoad(e);
        }

        private static void UpdateOrder(IList<IPageTeaser> pageTeasers, string serializedOrder)
        {
            var newOrder = ParseNewOrder(serializedOrder);

            foreach (var dataScope in new[] { DataScopeIdentifier.Administrated, DataScopeIdentifier.Public })
            {
                using (new DataScope(dataScope))
                {
                    foreach (var instance in pageTeasers)
                    {
                        string number = HashId(instance);

                        if (!newOrder.ContainsKey(number) || newOrder[number] == instance.LocalOrdering)
                        {
                            continue;
                        }

                        instance.LocalOrdering = newOrder[number];

                        DataFacade.Update(instance);
                    }
                }
            }
        }
    }
}
