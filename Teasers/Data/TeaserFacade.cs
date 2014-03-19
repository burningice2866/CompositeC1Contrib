using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.Data
{
    public class TeaserFacade
    {
        private static readonly List<Type> PageTeaserTypes = new List<Type>();
        private static readonly List<Type> SharedTeaserTypes = new List<Type>();

        static TeaserFacade()
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                try
                {
                    var pageTeaserTypes = asm.GetTypes()
                        .Where(t => t.IsInterface && typeof(IPageTeaser).IsAssignableFrom(t)
                                    && t.GetCustomAttributes(typeof(ImmutableTypeIdAttribute), false).Any());

                    PageTeaserTypes.AddRange(pageTeaserTypes);

                    var sharedTeaserTypes = asm.GetTypes()
                        .Where(t => t.IsInterface && typeof(ISharedTeaser).IsAssignableFrom(t)
                                    && t.GetCustomAttributes(typeof(ImmutableTypeIdAttribute), false).Any());

                    SharedTeaserTypes.AddRange(sharedTeaserTypes);
                }
                catch { }
            }
        }

        public static IList<PageTeaserHolder> PageTeasersForRequest
        {
            get
            {
                var ctx = HttpContext.Current;
                var list = ctx.Items["teasers"] as IList<PageTeaserHolder>;

                if (list == null)
                {
                    list = new List<PageTeaserHolder>();

                    ctx.Items.Add("teasers", list);
                }

                return list;
            }
        }

        public static IEnumerable<ISharedTeaser> GetSharedTeasers(Guid teaserGroup)
        {
            return GetSharedTeasers(teaserGroup, true);
        }

        public static IEnumerable<ISharedTeaser> GetSharedTeasers(Guid teaserGroup, bool filterPublished)
        {
            var list = new List<ISharedTeaser>();

            foreach (var type in SharedTeaserTypes)
            {
                IQueryable<ISharedTeaser> query = DataFacade.GetData(type).Cast<ISharedTeaser>().Where(t => t.TeaserGroup == teaserGroup);

                if (filterPublished)
                {
                    query = FilterPublishedDate(query);
                }

                list.AddRange(query);
            }

            return list;
        }

        public static IEnumerable<IPageTeaser> GetPageTeasers()
        {
            return GetPageTeasers(null, null, true);
        }

        public static IEnumerable<IPageTeaser> GetPageTeasers(IPage page)
        {
            return GetPageTeasers(page, null, true);
        }

        public static IEnumerable<IPageTeaser> GetPageTeasers(IPage page, string position, bool filterPublished)
        {
            var list = new List<IPageTeaser>();

            foreach (var type in PageTeaserTypes)
            {
                IQueryable<IPageTeaser> query = DataFacade.GetData(type).Cast<IPageTeaser>();

                if (page != null)
                {
                    query = query.Where(t => t.PageId == page.Id);
                }

                if (!String.IsNullOrEmpty(position))
                {
                    query = query.Where(t => t.Position == position);
                }

                if (filterPublished)
                {
                    query = FilterPublishedDate(query);
                }

                list.AddRange(query);
            }

            return list.OrderBy(t => t.Position).ThenBy(t => t.LocalOrdering);
        }

        public static IList<Type> GetPageTeaserTypes()
        {
            return PageTeaserTypes;
        }

        public static IList<Type> GetSharedTeaserTypes()
        {
            return SharedTeaserTypes;
        }

        private static IQueryable<T> FilterPublishedDate<T>(IQueryable<T> query) where T : ITeaser
        {
            var now = DateTime.Now;

            return query.Where(t =>
                        (t.PublishDate == null || t.PublishDate < now) &&
                        (t.UnpublishDate == null || t.UnpublishDate > now));
        }
    }
}
