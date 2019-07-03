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
        private static readonly MethodInfo SelectMethod = StaticReflection.GetGenericMethodInfo(() => Select<IGenericSortable>(null, null));

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

                if (typeof(IPageDataFolder).IsAssignableFrom(type))
                {
                    var pageId = Guid.Parse(sPageId);

                    data = data.OfType<IPageDataFolder>().Where(f => f.PageId == pageId);
                }

                if (!String.IsNullOrEmpty(sFilter))
                {
                    var filterArgs = new Dictionary<string, object>();

                    var filterFields = sFilter.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var field in filterFields)
                    {
                        var filterParts = field.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (filterParts.Length == 2)
                        {
                            filterArgs.Add(filterParts[0], filterParts[1]);
                        }
                    }

                    if (filterArgs.Any())
                    {
                        return GetInstancesWithFilter(data, type, filterArgs);
                    }
                }

                return GetInstancesWithoutFilter(data);
            }
        }

        private static IEnumerable<IGenericSortable> GetInstancesWithoutFilter(IQueryable data)
        {
            return data.OfType<IGenericSortable>().OrderBy(g => g.LocalOrdering);
        }

        private static IEnumerable<IGenericSortable> GetInstancesWithFilter(IQueryable data, Type type, Dictionary<string, object> filter)
        {
            var generic = SelectMethod.MakeGenericMethod(type);

            return (IQueryable<IGenericSortable>)generic.Invoke(null, new object[] { data, filter });
        }

        private static IQueryable<T> Select<T>(IQueryable<T> data, Dictionary<string, object> filter) where T : class, IGenericSortable
        {
            data = data.OrderBy(g => g.LocalOrdering);

            var dataType = typeof(T);

            foreach (var kvp in filter)
            {
                var propInfo = dataType.GetPropertiesRecursively(p => p.Name == kvp.Key).Single();
                var propType = propInfo.PropertyType;

                var paramExpr = Expression.Parameter(dataType);
                var propExpr = Expression.Property(paramExpr, propInfo);

                var value = ValueTypeConverter.Convert(kvp.Value, propType);

                var valueExpr = Expression.Constant(value);
                var equalExpr = Expression.Equal(propExpr, valueExpr);
                var lambda = Expression.Lambda<Func<T, bool>>(equalExpr, paramExpr);

                data = data.Where(lambda);
            }

            return data;
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
