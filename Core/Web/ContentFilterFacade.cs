using System;
using System.Collections.Generic;
using System.Linq;

using Composite.Core.Xml;

namespace CompositeC1Contrib.Web
{
    public static class ContentFilterFacade
    {
        private static readonly List<IContentFilter> ContentFilters;

        static ContentFilterFacade()
        {
            ContentFilters = new List<IContentFilter>();

            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                try
                {
                    var types = asm.GetTypes()
                        .Where(t => typeof(IContentFilter).IsAssignableFrom(t) && !t.IsInterface)
                        .Select(t => (IContentFilter)Activator.CreateInstance(t));

                    ContentFilters.AddRange(types);
                }
                catch { }
            }
        }

        public static XhtmlDocument FilterContent(XhtmlDocument doc)
        {
            return FilterContent(doc, String.Empty);
        }

        public static XhtmlDocument FilterContent(XhtmlDocument doc, string id)
        {
            if (doc == null)
            {
                return null;
            }

            foreach (var filter in ContentFilters)
            {
                filter.Filter(doc, id);
            }

            return doc;
        }
    }
}
