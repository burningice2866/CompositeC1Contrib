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
            updateParents(serializedEntityToken, consoleId);
        }

        protected IEnumerable<IPage> getPages()
        {
            var pageId = Guid.Parse(Request.QueryString["pageId"]);

            using (var data = new DataConnection(PublicationScope.Unpublished))
            {
                var pages = data.Get<IPage>();
                var structure = data.Get<IPageStructure>().Where(s => s.ParentId == pageId).OrderBy(s => s.LocalOrdering);

                return structure.Join(pages, s => s.Id, p => p.Id, (s, p) => p);
            }
        }

        private static void UpdateOrder(Guid pageId, string serializedOrder)
        {
            var newOrder = new Dictionary<string, int>();

            serializedOrder = serializedOrder.Replace("instance[]=", ",").Replace("&", "");
            var split = serializedOrder.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                newOrder.Add(split[i], i);
            }

            foreach (var dataScope in new[] { DataScopeIdentifier.Administrated, DataScopeIdentifier.Public })
            {
                using (var data = new DataConnection(PublicationScope.Unpublished))
                {
                    var instances = data.Get<IPageStructure>().Where(s => s.ParentId == pageId).OrderBy(s => s.LocalOrdering);

                    foreach (var instance in instances)
                    {
                        string number = hashId(instance);

                        if (newOrder.ContainsKey(number) && newOrder[number] != instance.LocalOrdering)
                        {
                            instance.LocalOrdering = newOrder[number];

                            data.Update(instance);
                        }
                    }
                }
            }
        }
    }
}
