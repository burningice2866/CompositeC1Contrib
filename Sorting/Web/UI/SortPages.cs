using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Sorting.Web.UI
{
    public class SortPages : BaseSortPage
    {
        [WebMethod]
        public static void UpdateOrder(string pageId, string consoleId, string entityToken, string serializedOrder)
        {
            UpdateOrder(Guid.Parse(pageId), serializedOrder);

            var serializedEntityToken = HttpUtility.UrlDecode(entityToken);
            UpdateParents(serializedEntityToken, consoleId);
        }

        protected override void OnLoad(EventArgs e)
        {
            var pageId = Request.QueryString["pageId"];

            if (Request.HttpMethod == "POST")
            {
                pageId = Request.Form["pageId"];
            }

            Master.CustomJsonDataName = "pageId";
            Master.CustomJsonDataValue = pageId;

            Master.SortableItems = GetChildPages(Guid.Parse(pageId)).Select(i => new SortableItem
            {
                Id = HashId(i),
                Name = i.GetLabel()
            });

            base.OnLoad(e);
        }

        protected IEnumerable<IPage> GetChildPages(Guid pageId)
        {
            using (var data = new DataConnection(PublicationScope.Unpublished))
            {
                var pages = data.Get<IPage>();
                var structure = data.Get<IPageStructure>().Where(s => s.ParentId == pageId).OrderBy(s => s.LocalOrdering);

                return structure.Join(pages, s => s.Id, p => p.Id, (s, p) => p);
            }
        }

        private static void UpdateOrder(Guid pageId, string serializedOrder)
        {
            var newOrder = ParseNewOrder(serializedOrder);

            using (var data = new DataConnection(PublicationScope.Unpublished))
            {
                var instances = data.Get<IPageStructure>().Where(s => s.ParentId == pageId).OrderBy(s => s.LocalOrdering);
                foreach (var instance in instances)
                {
                    var number = HashId(instance);
                    if (!newOrder.ContainsKey(number) || newOrder[number] == instance.LocalOrdering)
                    {
                        continue;
                    }

                    instance.LocalOrdering = newOrder[number];

                    data.Update(instance);
                }
            }
        }
    }
}
