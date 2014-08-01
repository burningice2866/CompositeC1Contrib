using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                UpdateParents(serializedEntityToken, consoleId);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            var pageId = Request.QueryString["pageId"];
            var type = Request.QueryString["type"];
            var filter = Request.QueryString["filter"];

            if (Request.HttpMethod == "POST")
            {
                pageId = Request.Form["pageId"];
                type = Request.Form["type"];
                filter = Request.Form["filter"];
            }

            Master.CustomJsonDataName = "type";
            Master.CustomJsonDataValue = type;

            Master.SortableItems = GetInstances(pageId, type, filter).Select(i => new SortableItem
            {
                Id = HashId(i),
                Name = i.GetLabel()
            });

            base.OnLoad(e);
        }

        protected IEnumerable<IGenericSortable> GetInstances(string sPageId, string sType, string sFilter)
        {
            sType = HttpUtility.UrlDecode(sType);
            sFilter = sFilter != null ? HttpUtility.UrlDecode(sFilter) : String.Empty;
            var type = TypeManager.GetType(sType);

            using (new DataScope(DataScopeIdentifier.Administrated))
            {
                var instances = DataFacade.GetData(type).Cast<IGenericSortable>();

                if (typeof(IPageFolderData).IsAssignableFrom(type))
                {
                    var pageId = Guid.Parse(sPageId);

                    instances = instances.Cast<IPageFolderData>().Where(f => f.PageId == pageId).Cast<IGenericSortable>();
                }

                instances = instances.OrderBy(g => g.LocalOrdering);

                if (String.IsNullOrEmpty(sFilter))
                {
                    return instances;
                }

                var filterParts = sFilter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (filterParts.Length != 2)
                {
                    return instances;
                }

                var param = Expression.Parameter(type);
                var prop = Expression.Property(param, filterParts[0]);
                var value = Expression.Constant(filterParts[1]);
                var equal = Expression.Equal(prop, value);
                var lambda = Expression.Lambda<Func<IGenericSortable, bool>>(equal, param);

                instances = instances.Where(lambda);

                return instances;
            }
        }

        private static void UpdateOrder(Type type, string serializedOrder)
        {
            var newOrder = ParseNewOrder(serializedOrder);

            foreach (var dataScope in new[] { DataScopeIdentifier.Administrated, DataScopeIdentifier.Public })
            {
                using (new DataScope(dataScope))
                {
                    var instances = DataFacade.GetData(type).Cast<IGenericSortable>().ToList();

                    foreach (var instance in instances)
                    {
                        var number = HashId(instance);
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
