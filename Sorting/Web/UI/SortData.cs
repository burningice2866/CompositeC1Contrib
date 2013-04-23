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
            var sType = HttpUtility.UrlDecode(type);

            UpdateOrder(TypeManager.GetType(sType), serializedOrder);

            var serializedEntityToken = HttpUtility.UrlDecode(entityToken);
            if (!String.IsNullOrEmpty(serializedEntityToken))
            {
                updateParents(serializedEntityToken, consoleId);
            }
        }

        protected IEnumerable<IGenericSortable> getInstances()
        {
            var sType = HttpUtility.UrlDecode(Request.QueryString["type"]);
            var sFilter = Request.QueryString["filter"] != null ? HttpUtility.UrlDecode(Request.QueryString["filter"]) : String.Empty;
            var type = TypeManager.GetType(sType);

            using (new DataScope(DataScopeIdentifier.Administrated))
            {
                IEnumerable<IGenericSortable> instances = DataFacade.GetData(type).Cast<IGenericSortable>();

                if (typeof(IPageFolderData).IsAssignableFrom(type))
                {
                    var pageId = Guid.Parse(Request.QueryString["pageId"]);

                    instances = instances.Cast<IPageFolderData>().Where(f => f.PageId == pageId).Cast<IGenericSortable>();
                }

                instances = instances.OrderBy(g => g.LocalOrdering);

                if (!String.IsNullOrEmpty(sFilter))
                {
                    var filterParts = sFilter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (filterParts.Length == 2)
                    {
                        instances = instances.AsEnumerable().Where(d =>
                        {
                            var prop = type.GetProperty(filterParts[0]);
                            var value = filterParts[1];

                            return prop.GetValue(d, null).ToString() == value;
                        });
                    }
                }

                return instances;
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
                    var instances = DataFacade.GetData(type).Cast<IGenericSortable>().ToList();

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
