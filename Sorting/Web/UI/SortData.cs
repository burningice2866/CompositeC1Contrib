using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Services;

using Composite.Core.Types;
using Composite.Data;

namespace CompositeC1Contrib.Sorting.Web.UI
{
    public class SortData : BaseSortPage
    {
        private static readonly MethodInfo SelectMethod = StaticReflection.GetGenericMethodInfo(() => Select<IGenericSortable>(null, null, null));

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
                var data = DataFacade.GetData(type);

                if (typeof(IPageFolderData).IsAssignableFrom(type))
                {
                    var pageId = Guid.Parse(sPageId);

                    data = data.OfType<IPageFolderData>().Where(f => f.PageId == pageId);
                }

                if (String.IsNullOrEmpty(sFilter))
                {
                    return GetInstancesWithoutFilter(data);
                }

                var filterParts = sFilter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (filterParts.Length != 2)
                {
                    return GetInstancesWithoutFilter(data);
                }

                return GetInstancesWithFilter(data, type, filterParts[0], filterParts[1]);
            }
        }

        private static IEnumerable<IGenericSortable> GetInstancesWithoutFilter(IQueryable data)
        {
            return data.OfType<IGenericSortable>().OrderBy(g => g.LocalOrdering);
        }

        private static IEnumerable<IGenericSortable> GetInstancesWithFilter(IQueryable data, Type type, string field, object value)
        {
            var generic = SelectMethod.MakeGenericMethod(type);

            return (IQueryable<IGenericSortable>)generic.Invoke(null, new[] { data, field, value });
        }

        private static IQueryable<T> Select<T>(IQueryable<T> data, string field, object value) where T : class, IGenericSortable
        {
            data = data.OrderBy(g => g.LocalOrdering);

            var paramExpr = Expression.Parameter(typeof(T));
            var propExpr = Expression.Property(paramExpr, field);
            var valueExpr = Expression.Constant(value);
            var equalExpr = Expression.Equal(propExpr, valueExpr);

            var lambda = Expression.Lambda<Func<T, bool>>(equalExpr, paramExpr);

            return data.Where(lambda);
        }

        private static void UpdateOrder(Type type, string serializedOrder)
        {
            var newOrder = ParseNewOrder(serializedOrder);

            foreach (var dataScope in new[] { DataScopeIdentifier.Administrated, DataScopeIdentifier.Public })
            {
                using (new DataScope(dataScope))
                {
                    var instances = DataFacade.GetData(type).OfType<IGenericSortable>().ToList();

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
