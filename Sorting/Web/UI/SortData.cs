using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using Composite.Core.Types;
using Composite.Data;

namespace CompositeC1Contrib.Sorting.Web.UI
{
    public class SortData : BaseSortPage
    {
        [WebMethod]
        public static void UpdateOrder(string type, string consoleId, string entityToken, string serializedOrder)
        {
            var s = HttpUtility.UrlDecode(type);

            UpdateOrder(TypeManager.GetType(s), serializedOrder);

            var serializedEntityToken = HttpUtility.UrlDecode(entityToken);
            updateParents(serializedEntityToken, consoleId);
        }

        protected IEnumerable<IGenericSortable> getInstances()
        {
            var s = HttpUtility.UrlDecode(Request.QueryString["type"]);
            var type = TypeManager.GetType(s);

            using (new DataScope(DataScopeIdentifier.Administrated))
            {
                var instances = DataFacade.GetData(type).Cast<IGenericSortable>();

                if (typeof(IPageFolderData).IsAssignableFrom(type))
                {
                    var pageId = Guid.Parse(Request.QueryString["pageId"]);

                    instances = instances.Cast<IPageFolderData>().Where(f => f.PageId == pageId).Cast<IGenericSortable>();
                }

                return instances.OrderBy(g => g.LocalOrdering);
            }
        }        

        private static void UpdateOrder(Type type, string serializedOrder)
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
                using (new DataScope(dataScope))
                {
                    var instances = DataFacade.GetData(type).Cast<IGenericSortable>();

                    foreach (var instance in instances)
                    {
                        string number = hashId(instance);

                        if (newOrder.ContainsKey(number) && newOrder[number] != instance.LocalOrdering)
                        {
                            instance.LocalOrdering = newOrder[number];

                            DataFacade.Update(instance);
                        }
                    }
                }
            }
        }
    }
}
